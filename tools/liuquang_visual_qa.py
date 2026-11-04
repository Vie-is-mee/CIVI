"""Targeted final-build visual regression using genuine saves from native-input QA."""
from pathlib import Path
shared=Path(__file__).with_name('chapter1_windows_qa.py').read_text(encoding='utf-8')
shared=shared.replace('a=p.parse_args()',"p.add_argument('--checkpoints',required=True);a=p.parse_args()")
exec(compile(shared.split('def story(expected):')[0],str(Path(__file__).with_name('chapter1_windows_qa.py')),'exec'))
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
    for _ in range(200):
        u.EnumWindows(callback(find),0)
        if hwnd:break
        time.sleep(.2)
    if not hwnd:raise RuntimeError('No player window')
    until(lambda s:s['help'] and s['frame']>30,60);focus();size_window()
    for name,step in [('00-dialogue',0),('05',5),('07',7),('09',9)]:
        checkpoint=Path(a.checkpoints)/f'checkpoint-{name}.json'
        original=json.loads(checkpoint.read_text(encoding='utf-8-sig'))
        assert original['step']==step
        shutil.copy2(checkpoint,out/'c1-save.json');key(0x78)
        until(lambda s:s['step']==step and not s['help']);time.sleep(1)
        capture(f'final-{name}')
        record('genuine-checkpoint-resume',source=str(checkpoint),step=step)
    record('VISUAL-PASS',resolution=[a.width,a.height])
except Exception as e:
    record('FAIL',error=str(e),traceback=traceback.format_exc());raise
finally:
    if proc and proc.poll() is None:
        if hwnd:u.PostMessageW(hwnd,0x0010,0,0)
        try:proc.wait(timeout=10)
        except subprocess.TimeoutExpired:proc.terminate()
