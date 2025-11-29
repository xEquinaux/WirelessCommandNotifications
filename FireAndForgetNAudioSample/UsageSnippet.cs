// on startup:
var zap = new CachedSound("zap.wav");
var boom = new CachedSound("boom.wav");


// later in the app...
AudioFileEngine.Instance.PlaySound(zap);
AudioFileEngine.Instance.PlaySound(boom);
AudioFileEngine.Instance.PlaySound("crash.wav");

// on shutdown
AudioFileEngine.Instance.Dispose();
