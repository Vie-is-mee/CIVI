"""Black-box Windows input QA. Uses real Win32 keyboard/mouse events, never calls story commands.

The -civi-qa player flag only enables read-only telemetry, isolated saves and F12 screenshots.
"""
import argparse,ctypes,json,math,os,subprocess,time,traceback,sys,shutil
sys.stdout.reconfigure(encoding='utf-8')
from ctypes import wintypes as W
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--exe',required=True);p.add_argument('--out',required=True);p.add_argument('--width',type=int,default=1280);p.add_argument('--height',type=int,default=720);p.add_argument('--keep',action='store_true');p.add_argument('--inspect',action='store_true');p.add_argument('--walk-test',action='store_true');a=p.parse_args()
out=Path(a.out).resolve();out.mkdir(parents=True,exist_ok=True)
u=ctypes.windll.user32;u.SetProcessDPIAware()
u.GetForegroundWindow.restype=W.HWND
u.SetForegroundWindow.argtypes=[W.HWND]
u.GetClientRect.argtypes=[W.HWND,ctypes.POINTER(W.RECT)]
u.ClientToScreen.argtypes=[W.HWND,ctypes.POINTER(W.POINT)]
u.GetWindowThreadProcessId.argtypes=[W.HWND,ctypes.POINTER(W.DWORD)]
u.SetWindowPos.argtypes=[W.HWND,W.HWND,ctypes.c_int,ctypes.c_int,ctypes.c_int,ctypes.c_int,W.UINT]
trace=[];proc=None;hwnd=None
class MOUSEINPUT(ctypes.Structure):_fields_=[('dx',W.LONG),('dy',W.LONG),('mouseData',W.DWORD),('dwFlags',W.DWORD),('time',W.DWORD),('dwExtraInfo',ctypes.c_size_t)]
class KEYBDINPUT(ctypes.Structure):_fields_=[('wVk',W.WORD),('wScan',W.WORD),('dwFlags',W.DWORD),('time',W.DWORD),('dwExtraInfo',ctypes.c_size_t)]
class INPUTUNION(ctypes.Union):_fields_=[('mi',MOUSEINPUT),('ki',KEYBDINPUT)]
class INPUT(ctypes.Structure):_anonymous_=('data',);_fields_=[('type',W.DWORD),('data',INPUTUNION)]
def sendkey(vk,up=False):
    event=INPUT(type=1);event.ki=KEYBDINPUT(0,u.MapVirtualKeyW(vk,0),8|(2 if up else 0)|(1 if vk in (0x25,0x26,0x27,0x28) else 0),0,0)
    if u.SendInput(1,ctypes.byref(event),ctypes.sizeof(INPUT))!=1:raise RuntimeError('SendInput rejected keyboard event')
def record(name,**values):
    entry={'test':name,'time':round(time.time(),3),**values};trace.append(entry);print(json.dumps({k:v for k,v in entry.items() if k not in ('state','traceback')},ensure_ascii=False),flush=True)
    (out/'input-trace.json').write_text(json.dumps(trace,ensure_ascii=False,indent=2),encoding='utf8')
def state():
    for _ in range(400):
        try:return json.loads((out/'state.json').read_text(encoding='utf-8-sig'))
        except (FileNotFoundError,json.JSONDecodeError):time.sleep(.1)
    raise RuntimeError('No player telemetry')
def until(predicate,timeout=15):
    end=time.time()+timeout
    while time.time()<end:
        s=state()
        if predicate(s):return s
        time.sleep(.12)
    from PIL import ImageGrab
    rect=W.RECT();u.GetClientRect(hwnd,ctypes.byref(rect));pt=W.POINT(0,0);u.ClientToScreen(hwnd,ctypes.byref(pt));ImageGrab.grab(bbox=(pt.x,pt.y,pt.x+rect.right,pt.y+rect.bottom)).save(out/'window-debug.png')
    raise AssertionError('Timed out: '+str(state()))
def focus(attempt=0):
    if u.GetForegroundWindow()==hwnd:return
    u.ShowWindow(hwnd,5)
    current=ctypes.windll.kernel32.GetCurrentThreadId()
    foreground=u.GetForegroundWindow();thread=u.GetWindowThreadProcessId(foreground,None)
    target=u.GetWindowThreadProcessId(hwnd,None)
    attached=[]
    for t in {thread,target}:
        if t and t!=current and u.AttachThreadInput(current,t,True):attached.append(t)
    try:u.BringWindowToTop(hwnd);u.SetForegroundWindow(hwnd);u.SetFocus(hwnd)
    finally:
        for t in attached:u.AttachThreadInput(current,t,False)
    time.sleep(.1)
    if u.GetForegroundWindow()!=hwnd:
        if attempt<8:time.sleep(.2);return focus(attempt+1)
        raise RuntimeError('Player does not have foreground focus; refusing to type into another app')
def size_window():
    rect=W.RECT(0,0,a.width,a.height);style=u.GetWindowLongW(hwnd,-16);extended=u.GetWindowLongW(hwnd,-20)
    u.AdjustWindowRectEx(ctypes.byref(rect),style,False,extended);w=rect.right-rect.left;h=rect.bottom-rect.top
    u.SetWindowPos(hwnd,None,(u.GetSystemMetrics(0)-w)//2,min(20,u.GetSystemMetrics(1)-h-40),w,h,0x14)
    until(lambda s:s['width']==a.width and s['height']==a.height,timeout=10)
def key(vk,duration=.075):
    focus();sendkey(vk)
    try:time.sleep(duration)
    finally:sendkey(vk,True)
    time.sleep(.14)
def click(x,y):
    focus();rect=W.RECT();u.GetClientRect(hwnd,ctypes.byref(rect));pt=W.POINT(round(x/1600*rect.right),round(y/900*rect.bottom));u.ClientToScreen(hwnd,ctypes.byref(pt));u.SetCursorPos(pt.x,pt.y);time.sleep(.15)
    u.mouse_event(2,0,0,0,0);time.sleep(.07);u.mouse_event(4,0,0,0,0);time.sleep(.22)
def capture(label):
    if state()['width']!=a.width or state()['height']!=a.height:size_window()
    before=set(out.glob('*.png'));key(0x7B);time.sleep(.8)
    after=set(out.glob('*.png'))-before
    if not after:
        from PIL import ImageGrab
        rect=W.RECT();u.GetClientRect(hwnd,ctypes.byref(rect));pt=W.POINT(0,0);u.ClientToScreen(hwnd,ctypes.byref(pt));ImageGrab.grab(bbox=(pt.x,pt.y,pt.x+rect.right,pt.y+rect.bottom)).save(out/'window-debug.png')
        raise AssertionError('Screenshot missing: '+label)
    f=next(iter(after));dest=out/(label+'.png');f.replace(dest);s=state();record('screenshot',label=label,path=str(dest),state=s)
    assert not s.get('textOverflows'), 'Text overflow: '+str(s.get('textOverflows'))
def go(x,y,arrows=True):
    start=state();speed=4.25 if 'Design' in start['scene'] else 6.4
    for axis,target in [('x',x),('y',y)]:
        for _ in range(60):
            s=state();d=target-s[axis]
            if abs(d)<.21:break
            vk=((0x27 if d>0 else 0x25) if axis=='x' else (0x26 if d>0 else 0x28)) if arrows else ((0x44 if d>0 else 0x41) if axis=='x' else (0x57 if d>0 else 0x53))
            key(vk,min(.6,max(.025,abs(d)/speed-.016)))
            n=state()
            if abs(n[axis]-s[axis])<.01:raise AssertionError('Movement blocked on route: '+str(n))
        else:raise AssertionError('Route did not converge')
    record('walked-with-arrows' if arrows else 'walked-with-WASD',start=[start['x'],start['y']],end=[state()['x'],state()['y']],target=[x,y])
def story(expected):
    saved=False;captured=False;chat=False;comparison=False
    for n in range(100):
        s=state()
        if s['phase']!=expected:return s
        if not saved:
            key(0x74);before=state();key(0x44,.35);after=state();assert math.dist([before['x'],before['y']],[after['x'],after['y']])<.03,'Movement during conversation'
            key(0x78);assert state()['line']==before['line'],'Load lost current line';saved=True;record('dialogue-lock-and-save-load',phase=expected,line=before['line'])
        if s['choices']:
            if not captured:capture(expected+'-choices');captured=True
            # Exercise mouse choice then keyboard 1-3. These buttons use the same regular UI as a player.
            if n<8:
                # First choice sits at the bottom panel; resolve bounds from read-only UI telemetry.
                buttons=s.get('buttons',[]);choices=[b for b in buttons if b['label'].startswith('1  /')]
                if choices:click(choices[0]['x'],choices[0]['y']);record('mouse-choice',label=choices[0]['label']);continue
            key(0x33 if len(s['choices'])==3 else 0x31)
        else:
            if expected=='Opening' and s['line']=='S00_L022':
                capture('s00-three-photos')
                photos=[b for b in s.get('buttons',[]) if b['label'].startswith('01 /')]
                if photos:
                    click(photos[0]['x'],photos[0]['y']);capture('s00-photo-inspection');key(0x1B);record('photo-mouse-open-escape-close')
            if s['line']=='S01_L005' and not chat:capture('s01-chat');chat=True
            if s['step']=='S01_COMPARE' and not comparison:capture('s01-comparison');comparison=True
            if n%3==0:
                button=next(b for b in s['buttons'] if b['label'].startswith('Tiếp tục'));click(button['x'],button['y']);record('mouse-advance',line=s['line'])
                if state()['line']==s['line']:key(0x0D)
            else:key(0x20 if n%2 else 0x0D)
        time.sleep(.08)
    raise AssertionError('Dialogue never finished')
try:
    cmd=[str(Path(a.exe).resolve()),'-screen-width',str(a.width),'-screen-height',str(a.height),'-screen-fullscreen','0','-civi-qa','-civi-qa-dir',str(out),'-logFile',str(out/'player.log')]
    if (out/'state.json').exists():(out/'state.json').unlink()
    proc=subprocess.Popen(cmd)
    callback=ctypes.WINFUNCTYPE(W.BOOL,W.HWND,W.LPARAM)
    def find(w,_):
        global hwnd
        pid=W.DWORD();u.GetWindowThreadProcessId(w,ctypes.byref(pid))
        if pid.value==proc.pid and u.IsWindowVisible(w):hwnd=w;return False
        return True
    for _ in range(150):
        u.EnumWindows(callback(find),0)
        if hwnd:break
        time.sleep(.2)
    if not hwnd:raise RuntimeError('Standalone window not found')
    until(lambda s:s['help'] and s['frame']>30,timeout=60);time.sleep(.5);u.EnumWindows(callback(find),0);size_window();record('started',pid=proc.pid,state=state());capture('01-onboarding')
    start_button=next(b for b in state()['buttons'] if b['label'].startswith('Bắt đầu'));click(start_button['x'],start_button['y']);until(lambda s:not s['help']);capture('02-courtyard-spawn')
    if a.walk_test:
        key(0x78);until(lambda s:s['phase']=='WalkToDoor');go(10,-3);go(10,4.3);capture('walk-test-door');record('WALK-PASS')
    elif a.inspect:
        record('INSPECT-READY');a.keep=True
    else:
        before=state();key(0x44,.15);record('WASD-single-D',distance=state()['x']-before['x'])
        go(-10,-2.8);key(0x45);until(lambda s:s['phase']=='Opening');capture('03-s00-dialogue')
        story('Opening');until(lambda s:s['phase']=='WalkToDoor');capture('04-s00-complete')
        go(10,-3);go(10,4.3);capture('05-design-door')
        key(0x74);before=state();go(8,4.3);key(0x78);until(lambda s:abs(s['x']-before['x'])<.1);record('exploration-save-load',phase=state()['phase'])
        key(0x45);until(lambda s:s['phase']=='StudioArrival');time.sleep(.7);capture('06-studio-entry')
        go(0,-3.5);key(0x26,1);blocked=state();key(0x26,.5);assert abs(state()['y']-blocked['y'])<.03,'Walked through design table';record('real-input-table-collision',y=state()['y']);key(0x45);until(lambda s:s['phase']=='DesignDialogue');capture('07-s01-dialogue')
        story('DesignDialogue');until(lambda s:s['complete']);capture('08-chapter-complete')
        key(0x78);until(lambda s:s['complete']);record('completion-save-load')
        # Close and reopen a fresh process, then resume completion from the onboarding UI.
        focus();u.PostMessageW(hwnd,0x0010,0,0);proc.wait(timeout=10)
        shutil.copy2(out/'player.log',out/'player-first-run.log')
        shutil.copy2(state()['savePath'],out/'completed-save.json')
        proc=subprocess.Popen(cmd);hwnd=None
        for _ in range(100):
            u.EnumWindows(callback(find),0)
            if hwnd:break
            time.sleep(.2)
        until(lambda s:s['help']);time.sleep(.6);key(0x78);until(lambda s:s['complete']);capture('09-relaunch-resumed');record('PASS-real-input-end-to-end',resolution=[state()['width'],state()['height']])
except Exception as e:
    record('FAIL',error=str(e),traceback=traceback.format_exc());raise
finally:
    for vk in (0x57,0x41,0x53,0x44,0x45,0x0D,0x20):u.keybd_event(vk,0,2,0)
    if proc and not a.keep and proc.poll() is None:
        if hwnd:u.PostMessageW(hwnd,0x0010,0,0)
        try:proc.wait(timeout=8)
        except subprocess.TimeoutExpired:proc.terminate()
