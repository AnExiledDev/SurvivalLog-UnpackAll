// Unpack All's web half. Runs in Root.html, the document every game page iframe sits in. Puts "Pick up" in the game's
// click menu on a furniture package in the yard; "Unpack all" in the inventory's Mods menu comes from Exiled's Toolkit.
// The plugin shows the result of a press as an Exiled's Toolkit toast. Talks to the plugin through vuplex.postMessage("slmod|unpackall|<verb>") and is
// driven by window.__slunpackall.
(function () {
  if (window.__slunpackall) return;

  var PREFIX = 'slmod|unpackall|';
  var BUSY_TIMEOUT_MS = 5000;

  var isBusy = false;
  var busyTimer = 0;

  function send(message) {
    try {
      if (window.vuplex) window.vuplex.postMessage(PREFIX + message);
    } catch (e) {}
  }

  // One press at a time: a double click would otherwise ask twice. Every press is answered by the plugin, which
  // frees the entry; the timer frees it anyway if that answer never comes.
  function press(verb) {
    if (isBusy) return;

    isBusy = true;
    clearTimeout(busyTimer);
    busyTimer = setTimeout(clearBusy, BUSY_TIMEOUT_MS);
    send(verb);
  }

  function clearBusy() {
    isBusy = false;
    clearTimeout(busyTimer);
  }

  // The plugin answers every press once it has shown the result, which frees the entry.
  function answered() {
    clearBusy();
  }

  // ----- Pick up: an entry in the game's click menu on a furniture package in the yard -----

  var PICK_CLASS = 'slua-pickup';
  var MENU_PAGE = /SelectMenu\.html/;
  var SVG_NS = 'http://www.w3.org/2000/svg';
  // An arrow rising out of an open box, drawn on the 24-unit grid the game's toolbar icons use.
  var ICON_PATH = 'M4 10h4v2H6v8h12v-8h-2v-2h4v12H4zM12 2l5 5h-3v8h-4V7H7z';

  // Set by the plugin each time the click menu opens. why: null for no entry, '' for a working one, otherwise the
  // reason it is greyed out, shown in the menu's own hover tip in place of the usual tip.
  var pick = { why: null, label: 'Pick up', tip: '' };

  // The root's iframes, kept up to date by the browser, so a check never has to search the root document for them.
  var frames = document.getElementsByTagName('iframe');

  function pickMenu(why, label, tip) {
    pick = { why: why == null ? null : String(why), label: label || 'Pick up', tip: tip || '' };
    patchMenusSafely();
  }

  // Whether an iframe shows the click menu, worked out again only when its address changes.
  function isMenuFrame(frame) {
    var src = frame.src || '';

    if (frame.uaSrc !== src) {
      frame.uaSrc = src;
      frame.uaIsMenu = MENU_PAGE.test(src);
    }

    return frame.uaIsMenu;
  }

  function patchMenus() {
    for (var i = 0; i < frames.length; i++) {
      var frame = frames.item(i);

      if (!isMenuFrame(frame)) continue;

      var doc = null;

      try {
        doc = frame.contentDocument;
      } catch (e) {}

      if (doc) patchMenu(doc);
    }
  }

  function patchMenusSafely() {
    try {
      patchMenus();
    } catch (e) {}
  }

  // Shown last in the menu's main column while it is open on a yard package. The press names no package: the plugin
  // picks up whatever the menu is open on at that moment. With the menu not on a yard package (most of the time) this
  // searches nothing: the entry placed is kept on the menu's document.
  function patchMenu(doc) {
    watchMenu(doc);

    var existing = doc.uaPickEntry && doc.uaPickEntry.isConnected ? doc.uaPickEntry : null;

    if (pick.why === null) {
      if (existing) existing.remove();

      return;
    }

    var key = pick.why + '|' + pick.label + '|' + pick.tip;

    // The common poll: the entry is drawn and still sits with the game's entries, so nothing is searched.
    if (existing && existing.getAttribute('data-key') === key && existing.parentNode.children.length > 1) return;

    var container = doc.getElementById('menu-container');
    var hasGameEntries = container != null && container.querySelectorAll('.menu-btn').length > (existing ? 1 : 0);

    if (!hasGameEntries) {
      if (existing) existing.remove();

      return;
    }

    if (existing && existing.getAttribute('data-key') === key) return;
    if (existing) existing.remove();

    doc.uaPickEntry = pickEntry(doc, key);
    (container.querySelector('.menu-col') || container).appendChild(doc.uaPickEntry);
  }

  // The menu draws its entries a moment after it opens; adding Pick up the moment they appear, rather than at the
  // next poll, keeps it from showing up after the player has already moved to click.
  function watchMenu(doc) {
    if (doc.uaPickWatched || !doc.body || typeof MutationObserver === 'undefined') return;

    doc.uaPickWatched = true;
    new MutationObserver(function () {
      try {
        patchMenu(doc);
      } catch (e) {}
    }).observe(doc.body, { childList: true, subtree: true });
  }

  // The game's own entry markup, so its stylesheet draws it and its hover tip like the rest.
  function pickEntry(doc, key) {
    var why = pick.why;
    var node = doc.createElement('div');
    var svg = doc.createElementNS(SVG_NS, 'svg');
    var path = doc.createElementNS(SVG_NS, 'path');
    var text = doc.createElement('span');
    var tooltip = doc.createElement('div');
    var tipText = doc.createElement('div');

    node.className = 'menu-btn show ' + PICK_CLASS + (why ? ' locked' : '');
    node.setAttribute('data-interactive', '');
    node.setAttribute('data-key', key);
    svg.setAttribute('class', 'menu-btn-icon');
    svg.setAttribute('viewBox', '0 0 24 24');
    svg.setAttribute('fill', 'currentColor');
    path.setAttribute('d', ICON_PATH);
    svg.appendChild(path);
    text.className = 'menu-btn-text';
    text.textContent = pick.label;
    tooltip.className = 'menu-btn-tooltip';
    tipText.className = 'tip-text';
    tipText.textContent = why || pick.tip;
    tooltip.appendChild(tipText);
    node.appendChild(svg);
    node.appendChild(text);
    node.appendChild(tooltip);
    node.addEventListener('click', function (e) {
      e.stopPropagation();

      if (!why) press('pickup');
    });

    return node;
  }

  // A click menu in a new iframe is watched as soon as the iframe appears and again when it loads, not at the next
  // poll, so the watcher is in place before the game draws the menu's entries. Any change in the root lands here, so
  // the check walks the kept list of iframes and searches nothing.
  function watchForFrames() {
    if (typeof MutationObserver === 'undefined') return;

    new MutationObserver(function () {
      var isNew = false;

      for (var i = 0; i < frames.length; i++) {
        var frame = frames.item(i);

        if (frame.uaLoadWatched) continue;

        frame.uaLoadWatched = true;
        frame.addEventListener('load', patchMenusSafely);
        isNew = true;
      }

      if (isNew) patchMenusSafely();
    }).observe(document.body, { childList: true, subtree: true });
  }

  watchForFrames();

  // Polled because the game creates and drops its pages whenever it likes, and Vue drops the button on a re-render.
  setInterval(function () {
    patchMenusSafely();
  }, 400);

  window.__slunpackall = { answered: answered, pickMenu: pickMenu };
  send('ready');
})();
