mergeInto(LibraryManager.library, {
  AmbrosWriteSave: function(keyPointer, jsonPointer) {
    try {
      var key = UTF8ToString(keyPointer), json = UTF8ToString(jsonPointer);
      if (json.length > 131072) return 0;
      JSON.parse(json);
      var previous = {};
      try { previous = JSON.parse(localStorage.getItem(key) || '{}'); } catch (_) {}
      if (!previous || typeof previous !== 'object') previous = {};
      // One atomic localStorage write stores both the checkpoint and recovery copy.
      localStorage.setItem(key, JSON.stringify({current: json, backup: previous.current || previous.backup || null}));
      return 1;
    } catch (_) { return 0; }
  },
  AmbrosReadSave: function(keyPointer, backup) {
    var value = '';
    try {
      var record = JSON.parse(localStorage.getItem(UTF8ToString(keyPointer)) || '{}');
      value = backup === 2 ? (record.reset ? 'reset' : '') : (backup ? record.backup : record.current);
      if (typeof value !== 'string' || value.length > 131072) value = '';
    } catch (_) {}
    var size = lengthBytesUTF8(value || '') + 1, pointer = _malloc(size);
    stringToUTF8(value || '', pointer, size);
    return pointer;
  },
  AmbrosResetSave: function(keyPointer) {
    try {
      // Keep a marker so a legacy filesystem save cannot resurrect after reset.
      localStorage.setItem(UTF8ToString(keyPointer), JSON.stringify({reset: true}));
      return 1;
    } catch (_) { return 0; }
  },
  AmbrosInstallSaveHooks: function(objectPointer) {
    var objectName = UTF8ToString(objectPointer);
    if (Module.ambrosSaveHooks) Module.ambrosSaveHooks();
    var save = function() { try { SendMessage(objectName, 'SaveBeforeBrowserHide'); } catch (_) {} };
    var visibility = function() { if (document.visibilityState === 'hidden') save(); };
    document.addEventListener('visibilitychange', visibility);
    window.addEventListener('pagehide', save);
    Module.ambrosSaveHooks = function() {
      document.removeEventListener('visibilitychange', visibility);
      window.removeEventListener('pagehide', save);
    };
  }
});
