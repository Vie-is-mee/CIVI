"""Real Windows keyboard/mouse C1 QA, using the previously verified Win32 harness.
Telemetry is read-only. Never calls game state methods or edits saves.
"""
from pathlib import Path
shared=Path(__file__).with_name('chapter1_windows_qa.py').read_text(encoding='utf-8')
exec(compile(shared.split('def story(expected):')[0].replace("speed=4.25 if 'Design' in start['scene'] else 6.4","speed=4.25"),str(Path(__file__).with_name('chapter1_windows_qa.py')),'exec'))

def button(prefix):
    s=state();b=next(b for b in s['buttons'] if b['label'].startswith(prefix));click(b['x'],b['y']);return b['label']

def launch(cmd):
    global proc,hwnd
    proc=subprocess.Popen(cmd);hwnd=None
    callback=ctypes.WINFUNCTYPE(W.BOOL,W.HWND,W.LPARAM)
    def find(w,_):
        global hwnd
        pid=W.DWORD();u.GetWindowThreadProcessId(w,ctypes.byref(pid))
        if pid.value==proc.pid and u.IsWindowVisible(w):hwnd=w;return False
        return True
    for _ in range(200):
        u.EnumWindows(callback(find),0)
        if hwnd:break
        time.sleep(.2)
    if not hwnd:raise RuntimeError('Player window missing')
    until(lambda s:s['help'] and s['frame']>30,60);size_window();focus()

try:
    cmd=[str(Path(a.exe).resolve()),'-screen-width',str(a.width),'-screen-height',str(a.height),'-screen-fullscreen','0','-civi-qa','-civi-qa-dir',str(out),'-logFile',str(out/'player.log')]
    if (out/'state.json').exists():(out/'state.json').unlink()
    launch(cmd);capture('00-onboarding');button('Bắt đầu');until(lambda s:not s['help'])
    if a.inspect:
        capture('01-memory-workbench');record('INSPECT-READY');a.keep=True
    else:
        for step in range(13):
            until(lambda s:s['step']==step and not s['transition'],20)
            task=state()['task']
            if step>0:
                # Walk through an unobstructed corridor, then up to the actual target.
                if step==1:
                    before=state();key(0x44,.15);assert state()['x']>before['x']+.1;record('retained-WASD',dx=state()['x']-before['x'])
                go(state()['x'],-4.4);go(state()['targetX'],-4.4);go(state()['targetX'],state()['targetY'])
                assert state()['distance']<=2.2;capture(f'{step:02}-{task}-room')
                if step==1:

                    for _ in range(6):
                        key(0x26,.4)
                        if state()['y']>-.85:break
                    until(lambda s:-.85<s['y']<-.65);blocked=state();key(0x26,.6);until(lambda s:s['frame']>blocked['frame']+2);assert -.85<state()['y']<-.65,'Crossed table footprint';record('real-footprint-collision',y=state()['y']);key(0x28,.2)
                key(0x45);until(lambda s:s['mode']==1)
            capture(f'{step:02}-{task}-workbench')
            # Invalid early submission must not advance any task.
            key(0x0D);assert state()['mode']==1;record('missing-prerequisites-blocked',task=task)
            n=max(int(b['label'][0]) for b in state()['buttons'] if b['label'][:1].isdigit())
            for i in range(n):
                if i%2==0:button(str(i+1)+'  ·')
                else:key(0x31+i)
                until(lambda s:s['mask']&(1<<i)!=0)
                assert not state()['textOverflows'],state()['textOverflows']
                if step==3 and i==2:
                    key(0x74);saved=state();key(0x33);changed=state();assert changed['mask']!=saved['mask'];key(0x78);until(lambda s:s['mask']==changed['mask']);key(0x33);until(lambda s:s['mask']==saved['mask']);record('mid-plan-autosave-load')
            before=state();key(0x27,.3);assert math.dist([before['x'],before['y']],[state()['x'],state()['y']])<.03;record('workbench-movement-lock',task=task)
            if step in (2,3,6,8,10,11,12):capture(f'{step:02}-{task}-inspected')
            button('Gửi yêu cầu' if step==11 else 'Kiểm tra phương án' if step==3 else 'Xác nhận');until(lambda s:s['mode']==2);capture(f'{step:02}-{task}-dialogue')
            if step==3:
                key(0x74);key(0x0D);s=state();key(0x78);until(lambda n:n['line']==s['line']);record('dialogue-bookmark-roundtrip')
            for _ in range(15):
                s=state()
                if s['step']!=step:break
                assert s['mode']==2
                if s['line']%2:button('Tiếp tục')
                else:key(0x0D)
                time.sleep(.07)
            else:raise AssertionError('Task did not finish')
        until(lambda s:s['complete']);capture('13-complete');record('full-input-playthrough-pass')
        key(0x74);shutil.copy2(state()['savePath'],out/'completed-save.json')
        focus();u.PostMessageW(hwnd,0x0010,0,0);proc.wait(timeout=15);shutil.copy2(out/'player.log',out/'player-first-run.log')
        if (out/'state.json').exists():(out/'state.json').unlink()
        launch(cmd);button('Tiếp tục bản lưu');until(lambda s:s['complete']);capture('14-relaunch-resumed');record('PASS',resolution=[a.width,a.height])
except Exception as e:
    record('FAIL',error=str(e),traceback=traceback.format_exc());raise
finally:
    if proc and not a.keep and proc.poll() is None:
        if hwnd:u.PostMessageW(hwnd,0x0010,0,0)
        try:proc.wait(timeout=10)
        except subprocess.TimeoutExpired:proc.terminate()
