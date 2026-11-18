// Microphone recording for the speech page (MediaRecorder). Exposed as window.siloSpeech.
window.siloSpeech = (() => {
    let recorder = null;
    let stream = null;
    let chunks = [];

    const pickMime = () => {
        const candidates = ['audio/webm;codecs=opus', 'audio/webm', 'audio/mp4', 'audio/ogg;codecs=opus'];
        return candidates.find(m => window.MediaRecorder && MediaRecorder.isTypeSupported(m)) || '';
    };

    const release = () => {
        if (stream) stream.getTracks().forEach(t => t.stop());
        stream = null;
        recorder = null;
        chunks = [];
    };

    return {
        isSupported: () => !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder),

        // Resolves true when recording started, false when the microphone is unavailable/denied.
        start: async () => {
            try {
                stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                const mime = pickMime();
                recorder = mime ? new MediaRecorder(stream, { mimeType: mime }) : new MediaRecorder(stream);
                chunks = [];
                recorder.ondataavailable = e => { if (e.data && e.data.size > 0) chunks.push(e.data); };
                recorder.start();
                return true;
            } catch (e) {
                release();
                return false;
            }
        },

        // Resolves { data: <base64>, mime } or null when nothing was recorded.
        stop: () => new Promise(resolve => {
            if (!recorder) { resolve(null); return; }
            const mime = recorder.mimeType || 'audio/webm';
            recorder.onstop = () => {
                const blob = new Blob(chunks, { type: mime });
                release();
                if (blob.size === 0) { resolve(null); return; }
                const reader = new FileReader();
                reader.onloadend = () => resolve({ data: String(reader.result).split(',')[1], mime });
                reader.readAsDataURL(blob);
            };
            recorder.stop();
        }),

        cancel: () => { if (recorder && recorder.state !== 'inactive') recorder.stop(); release(); }
    };
})();
