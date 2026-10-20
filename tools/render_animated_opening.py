"""Encode the 480 Unity-rendered animation frames and an original sound cue.
Run Capstone > Record Animated Opening first, then:
python3 tools/render_animated_opening.py /path/to/ffmpeg
"""
import array, math, pathlib, random, subprocess, sys, wave
base=pathlib.Path(__file__).resolve().parents[1]
work=base/'tools/.intro-animated'
rate=24000
rng=random.Random(19)
samples=array.array('h')
for n in range(rate*20):
    t=n/rate
    fade=min(1,t/.8,(20-t)/.7)
    root=130.81 if t<5 else 146.83 if t<14.5 else 196
    v=.025*math.sin(2*math.pi*root*t)+.012*math.sin(2*math.pi*root*1.5*t)
    # Delicate music-box notes, quiet typing and portal crescendo.
    notes=[523.25,659.25,783.99,659.25,587.33,783.99,880,1046.5]
    beat=t% .75;note=notes[int(t/.75)%len(notes)]
    v+=.035*math.exp(-beat*8)*math.sin(2*math.pi*note*t)
    if t<4.5 and t%.23<.018:v+=rng.uniform(-.018,.018)
    if 5<t<14.5:
        a=math.sin(math.pi*(t-5)/9.5)
        v+=a*(.035*math.sin(2*math.pi*(220*t+24*(t-5)**2))+.018*rng.uniform(-1,1))
    if 14.5<t<15:v+=.07*math.exp(-(t-14.5)*12)*math.sin(2*math.pi*85*t)
    samples.append(int(max(-1,min(1,v*fade))*32767))
with wave.open(str(work/'sound.wav'),'wb') as w:
    w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(samples.tobytes())
subprocess.run([sys.argv[1],'-y','-hide_banner','-loglevel','error','-framerate','24','-i',str(work/'%05d.jpg'),'-i',str(work/'sound.wav'),'-frames:v','480','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','128k','-movflags','+faststart',str(base/'capstone/Assets/Opening/Opening.mp4')],check=True)
