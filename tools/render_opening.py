"""Render generated cinematic keyframes into the game's automatic 34 second opening.
Usage: python3 tools/render_opening.py FFMPEG HOME_IMAGE PORTAL_IMAGE SPRING_IMAGE
Requires ffmpeg; uses only Python's standard library. No footage from Jumanji is used.
"""
import sys, subprocess, pathlib, math, wave, array
base=pathlib.Path(__file__).resolve().parents[1]
ffmpeg,*images=sys.argv[1:]
out=base/'capstone/Assets/Opening';out.mkdir(parents=True,exist_ok=True)
work=base/'tools/.opening-work';work.mkdir(exist_ok=True)
font='/System/Library/Fonts/Supplemental/Arial.ttf'
shots=[(11,[(0,5,'Ambros thought it was just another game.'),(5,11,'A quiet night. One laptop. One question about AI.')]),(9,[(0,4.5,'Then the screen began to change.'),(4.5,9,'Before he could react, the game pulled him inside.')]),(14,[(0,4,'Spring. Summer. Fall. Winter.'),(4,8,'Learn the lessons. Defeat each guardian.'),(8,11,'Beat the game... and find your way home.'),(11,14,"AMBROS' JOURNEY  /  SPRING")])]
for i,((duration,captions),image) in enumerate(zip(shots,images)):
    # Subtle camera pushes, letterboxing, readable captions and film fades.
    filters=["scale=1600:900:force_original_aspect_ratio=increase","crop=1600:900",f"zoompan=z='1+0.00016*on':x='iw/2-iw/zoom/2':y='ih/2-ih/zoom/2':d={duration*24}:s=1280x720:fps=24",'setsar=1','drawbox=x=0:y=0:w=iw:h=42:color=black:t=fill','drawbox=x=0:y=626:w=iw:h=94:color=black@0.9:t=fill']
    for n,(start,end,caption) in enumerate(captions):
        txt=work/f'caption-{i}-{n}.txt';txt.write_text(caption)
        filters.append(f"drawtext=fontfile={font}:textfile={txt}:fontsize=28:fontcolor=white:x=(w-tw)/2:y=655:enable='between(t,{start},{end})'")
    filters += ['fade=t=in:st=0:d=0.7',f'fade=t=out:st={duration-.7}:d=0.7','format=yuv420p']
    subprocess.run([ffmpeg,'-y','-hide_banner','-loglevel','error','-i',image,'-vf',','.join(filters),'-t',str(duration),'-c:v','libx264','-preset','fast','-crf','19',str(work/f'shot{i}.mp4')],check=True)
# Quiet original ambient cue; the portal has a rising shimmer.
audio=array.array('h');rate=24000;duration=sum(s[0] for s in shots)
for n in range(duration*rate):
    t=n/rate;fade=min(1,t/2,(duration-t)/2);amp=0.045*max(0,fade)
    chord=(130.81,164.81,196) if t<11 else (146.83,174.61,220) if t<20 else (130.81,196,261.63)
    value=sum(math.sin(2*math.pi*f*t)*(.65 if j==0 else .25) for j,f in enumerate(chord))
    if 11<t<20:value+=.25*math.sin(2*math.pi*(300*t+22*(t-11)**2))*math.sin(math.pi*(t-11)/9)
    audio.append(int(max(-1,min(1,value*amp))*32767))
with wave.open(str(work/'ambient.wav'),'wb') as wav:wav.setnchannels(1);wav.setsampwidth(2);wav.setframerate(rate);wav.writeframes(audio.tobytes())
(work/'concat.txt').write_text(''.join(f"file '{work/f'shot{i}.mp4'}'\n" for i in range(3)))
subprocess.run([ffmpeg,'-y','-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(work/'concat.txt'),'-i',str(work/'ambient.wav'),'-c:v','copy','-c:a','aac','-b:a','128k','-movflags','+faststart','-shortest',str(out/'Opening.mp4')],check=True)
print(out/'Opening.mp4')
