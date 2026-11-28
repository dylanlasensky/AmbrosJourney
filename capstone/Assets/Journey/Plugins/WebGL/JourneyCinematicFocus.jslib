// Bridge cinematic focus/fullscreen handling between Unity and its browser host.
mergeInto(LibraryManager.library, {
  AmbrosReturnCinematicFocus: function() {
    try {
      // Restore only the visible game after a film, never activate a background tab/window.
      if (document.visibilityState !== 'visible') return 0;
      var canvas = Module.canvas, active = document.activeElement;
      if (!canvas || !canvas.isConnected || typeof canvas.focus !== 'function') return 0;
      // Do not disturb a browser text field/control if one has already taken focus.
      if (active && active !== canvas && (active.isContentEditable || /^(INPUT|TEXTAREA|SELECT|BUTTON)$/.test(active.tagName))) return 0;
      canvas.focus({preventScroll: true});
      return document.activeElement === canvas ? 1 : 0;
    } catch (_) { return 0; }
  }
});
