// Unpack All's web half. Runs in Root.html, the document every game page iframe sits in. Adds an "Unpack all" button
// to the inventory's toolbar, styled by the game's own button classes so it reads as part of the game, and shows the
// result of a press as a short toast. Talks to the plugin through vuplex.postMessage("slmod|unpackall|<verb>") and is
// driven by window.__slunpackall.
(function () {
  if (window.__slunpackall) return;

  var PREFIX = 'slmod|unpackall|';
  var BUTTON_CLASS = 'ua-unpack-btn';
  var OTHER_MOD_BUTTONS = ['sh-putaway-btn', 'sh-floor-btn'];
  var BUSY_TIMEOUT_MS = 5000;
  var TOAST_MS = 3500;

  var isBusy = false;
  var busyTimer = 0;
  var toastTimer = 0;
  var hasReportedBackpackAccess = false;
  var hasReportedTabFallback = false;

  var STYLE = '#ua-toast{position:fixed;left:50%;bottom:120px;transform:translateX(-50%);z-index:2147483002;pointer-events:none;display:none;max-width:640px;text-align:center;padding:10px 18px;border-radius:8px;border:1px solid rgba(255,193,7,.75);background:linear-gradient(180deg,rgba(30,30,33,.94),rgba(18,18,20,.96));color:#eee;font-family:"Microsoft YaHei","PingFang SC",sans-serif;font-size:14px;box-shadow:0 6px 20px rgba(0,0,0,.6)}';

  function send(message) {
    try {
      if (window.vuplex) window.vuplex.postMessage(PREFIX + message);
    } catch (e) {}
  }

  // One press at a time: a double click would otherwise ask twice. Every press is answered by a status line, which
  // frees the button; the timer frees it anyway if that answer never comes.
  function sendPress() {
    if (isBusy) return;

    isBusy = true;
    clearTimeout(busyTimer);
    busyTimer = setTimeout(clearBusy, BUSY_TIMEOUT_MS);
    send('unpack');
  }

  function clearBusy() {
    isBusy = false;
    clearTimeout(busyTimer);
  }

  function toast() {
    if (!document.getElementById('ua-toast-style')) {
      var style = document.createElement('style');
      style.id = 'ua-toast-style';
      style.textContent = STYLE;
      document.head.appendChild(style);
    }

    var node = document.getElementById('ua-toast');

    if (node) return node;

    node = document.createElement('div');
    node.id = 'ua-toast';
    document.body.appendChild(node);

    return node;
  }

  function status(text) {
    clearBusy();

    var node = toast();

    // An empty status would be an empty box.
    if (!text) {
      node.style.display = 'none';

      return;
    }

    node.textContent = text;
    node.style.display = 'block';
    clearTimeout(toastTimer);
    toastTimer = setTimeout(function () { node.style.display = 'none'; }, TOAST_MS);
  }

  // Reads the inventory page's Vue state, which a game update can rename; failing open shows the button on every
  // tab, so the first failure is reported.
  function backpackTabWantsButton(doc) {
    try {
      var setup = doc.getElementById('app')._vnode.component.setupState;

      return setup.curATab === 0 && !setup.isPrologue;
    } catch (e) {
      if (!hasReportedTabFallback) send('diag|cannot read the inventory tab state, showing the button on every tab: ' + e);
      hasReportedTabFallback = true;

      return true;
    }
  }

  // The inventory page is a same-origin iframe, so its document can be reached from here; polled because the game
  // creates and drops that iframe whenever it likes.
  function patchBackpack() {
    var frames = document.querySelectorAll('iframe');

    for (var i = 0; i < frames.length; i++) {
      if (!/BackpackUI\.html/.test(frames[i].src || '')) continue;

      var doc = null;

      try {
        doc = frames[i].contentDocument;
      } catch (e) {}

      if (!doc) {
        if (!hasReportedBackpackAccess) send('diag|cannot reach the inventory page document (root ' + location.protocol + ', iframe ' + frames[i].src + ')');
        hasReportedBackpackAccess = true;
        continue;
      }

      placeButton(doc);
    }
  }

  function placeButton(doc) {
    var bar = doc.querySelector('.bag-slot .toolbar');
    var existing = doc.querySelector('.' + BUTTON_CLASS);
    var isWanted = !!bar && backpackTabWantsButton(doc);

    if (!isWanted) {
      if (existing) existing.remove();

      return;
    }

    if (existing && existing.parentNode === bar) return;
    if (existing) existing.remove();

    var node = doc.createElement('button');
    node.className = 'action-btn unity-interactive ' + BUTTON_CLASS;
    node.textContent = 'Unpack all';
    node.title = 'Unpack every package in your bag, then the ones on the floor at home, up to your free package spots';
    node.addEventListener('click', function (e) {
      e.stopPropagation();
      sendPress();
    });

    bar.insertBefore(node, firstGameButton(bar));
  }

  // The game's sort button, so ours sits just before it, after any other mod's buttons; the discard button is skipped.
  function firstGameButton(bar) {
    var buttons = bar.querySelectorAll('.action-btn');

    for (var i = 0; i < buttons.length; i++) {
      if (!buttons[i].classList.contains('discard-btn') && !isModButton(buttons[i])) return buttons[i];
    }

    return null;
  }

  function isModButton(button) {
    if (button.classList.contains(BUTTON_CLASS)) return true;

    for (var i = 0; i < OTHER_MOD_BUTTONS.length; i++) {
      if (button.classList.contains(OTHER_MOD_BUTTONS[i])) return true;
    }

    return false;
  }

  setInterval(function () {
    try {
      patchBackpack();
    } catch (e) {}
  }, 400);

  window.__slunpackall = { status: status };
  send('ready');
})();
