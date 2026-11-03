"""Generate original, short mono game cues. No third-party samples are used."""
from pathlib import Path
import math, random, struct, wave
RATE=32000
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'capstone/Assets/Audio/Resources/GameSounds'
OUT.mkdir(parents=True,exist_ok=True)
SPECS={'Click':(.030,.42),'Correct':(.075,.36),'Incorrect':(.095,.31),'Complete':(.20,.36),'Impact':(.085,.40),'Pickup':(.15,.33),'Portal':(.28,.25),'Dust':(.55,.24)}
for number,(name,(duration,peak)) in enumerate(SPECS.items()):
    rng=random.Random(493+number);values=[];low=slow=0
    for i in range(round(duration*RATE)):
        t=i/RATE;noise=rng.uniform(-1,1);low+=.20*(noise-low);slow+=.035*(noise-slow);dry=low-slow
        if name=='Click':v=(dry*.75+math.sin(2*math.pi*170*t)*.18)*math.exp(-t/0.0055)
        elif name=='Correct':v=(dry*.32+math.sin(2*math.pi*580*t)*.35+math.sin(2*math.pi*1170*t)*.10)*math.exp(-t/.016)
        elif name=='Incorrect':v=(slow*.65+math.sin(2*math.pi*155*t)*.30)*math.exp(-t/.023)
        elif name=='Complete':
            a=math.exp(-t/.036);b=math.exp(-max(0,t-.072)/.033) if t>=.072 else 0
            v=dry*(a*.45+b*.28)+.12*math.sin(2*math.pi*680*t)*a+.10*math.sin(2*math.pi*900*t)*b
        elif name=='Impact':v=(slow*.6+dry*.25+math.sin(2*math.pi*108*t)*.22)*math.exp(-t/.018)
        elif name=='Pickup':v=dry*(.35+.25*math.sin(2*math.pi*35*t)**2)*math.sin(math.pi*t/duration)**2+slow*.25
        elif name=='Portal':v=(low*.35+slow*.8)*math.sin(math.pi*t/duration)**2
        else:v=(dry*.25+slow*.5)*(.3+.7*math.sin(2*math.pi*31*t)**8)*math.sin(math.pi*t/duration)**1.5
        ramp=min(1,t/.0006,(duration-t)/.004);values.append(v*max(0,ramp))
    dc=sum(values)/len(values);values=[v-dc for v in values];scale=peak/max(abs(v) for v in values);values=[v*scale for v in values];values[0]=values[-1]=0
    with wave.open(str(OUT/(name+'.wav')),'wb') as f:
        f.setparams((1,2,RATE,len(values),'NONE','not compressed'));f.writeframes(b''.join(struct.pack('<h',round(v*32767)) for v in values))
    print(f'{name}: {duration:.3f}s, peak {max(abs(v) for v in values):.2f}')
