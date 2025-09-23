mergeInto(LibraryManager.library, {
  StartIstanbulClock: function(goNamePtr, methodPtr) {
    var goName = UTF8ToString(goNamePtr);
    var method = UTF8ToString(methodPtr);

    function tick() {
      // Europe/Istanbul saatini "HH:MM:SS" formatında al
      var s = new Intl.DateTimeFormat('tr-TR', {
        timeZone: 'Europe/Istanbul',
        hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
      }).format(new Date());

      // Unity'deki methoda string olarak gönder
      SendMessage(goName, method, s);
    }

    // Eski timer'ı temizle
    if (typeof window.__istanbulClockId !== 'undefined') {
      clearInterval(window.__istanbulClockId);
    }

    tick();
    window.__istanbulClockId = setInterval(tick, 1000);
  },

  StopIstanbulClock: function() {
    if (typeof window.__istanbulClockId !== 'undefined') {
      clearInterval(window.__istanbulClockId);
      delete window.__istanbulClockId;
    }
  }
});
