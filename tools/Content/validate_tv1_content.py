"""Validate TV1 authoring data and simulate authored paths; does not run Unity."""
from pathlib import Path
import copy
import json

ROOT = Path(__file__).resolve().parents[2]
CONTENT = ROOT / 'Game/Assets/_Civi/Content'
STORY = CONTENT / 'Story/v4_1'


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def check(condition, state):
    if 'all' in condition:
        return all(check(c, state) for c in condition['all'])
    if 'any' in condition:
        return any(check(c, state) for c in condition['any'])
    if 'not' in condition:
        return not check(condition['not'], state)
    actual = state[condition['field']]
    expected = condition['value']
    op = condition['op']
    if op == 'eq': return actual == expected
    if op == 'ne': return actual != expected
    if op == 'in': return actual in expected
    if op == 'contains': return expected in actual
    if op == 'gte': return actual >= expected
    if op == 'lt': return actual < expected
    raise AssertionError('Unknown condition: ' + op)


def apply(effects, state):
    for effect in effects:
        op = effect['op']
        if op == 'set':
            assert effect['field'] in state
            state[effect['field']] = copy.deepcopy(effect['value'])
        elif op == 'add_unique':
            target = state[effect['field']]
            assert isinstance(target, list)
            if effect['value'] not in target:
                target.append(copy.deepcopy(effect['value']))
        elif op == 'advance_minutes':
            assert effect['value'] >= 0
            state['courtyard_minute'] += effect['value']
            if state['courtyard_minute'] >= 70 and state['attendance'] == 'present':
                state['attendance'] = 'left'
                state['nhi_wants_capture'] = False
        elif op == 'grant':
            asset = effect.get('asset_id') or state[effect['asset_field']]
            scope = effect.get('scope') or state[effect['scope_field']]
            row = dict(asset_id=asset, subject_id=effect['subject_id'], scope=scope,
                       status=effect['status'], source_event_id=effect['source_event_id'])
            state['consent_grants'] = [g for g in state['consent_grants']
                if (g['asset_id'], g['subject_id'], g['scope']) != (asset, row['subject_id'], row['scope'])]
            state['consent_grants'].append(row)
        else:
            raise AssertionError('Unknown effect: ' + op)


scenes = {p.stem: read(p) for p in sorted((STORY/'scenes').glob('*.json'))}
dialogue = {p.stem: read(p) for p in sorted((STORY/'dialogue').glob('*.json'))}
initial = read(STORY/'initial-state.json')['state']
assets = {a['asset_id']: a for a in read(CONTENT/'Manifests/asset-manifest.json')['assets']}
speakers = {'AN', 'NHI', 'HA', 'MINH', 'VY', 'PHUONG', 'PRINTER', 'NARRATOR'}


def rights_ok(state, asset_id, scope):
    grants = state['consent_grants']
    return all(any(g['asset_id'] == asset_id and g['subject_id'] == subject
        and g['scope'] == scope and g['status'] == 'yes' for g in grants)
        for subject in assets[asset_id]['rights_holders'])


def validate_publication(state):
    assert state['page_previewed'] and state['equipment_packed']
    mode = state['final_mode']
    scope = 'supplement_delivery' if mode == 'supplement' else 'yearbook_print'
    assert rights_ok(state, state['final_old_asset'], scope), 'Old source permission missing'
    if mode == 'new':
        assert state['final_new_asset'] == state['captured_asset']
        assert rights_ok(state, state['final_new_asset'], scope), 'New photo permission missing'
    if state['use_0443']:
        assert rights_ok(state, 'PHOTO_0443', scope), '0443 permission missing'
    if mode == 'supplement':
        assert state['later_status'] == 'accepted'
        assert state['later_datetime'] and state['later_place'] and state['later_participants'] == ['AN','NHI']
    assert state['print_slot'] != 'extended' or state['extension_confirmed']
    if state['tribute_board_prepared'] and state['draft_retracted'] == 'yes':
        assert state['tribute_board_removed']
    if state['draft_retracted'] == 'yes':
        required = set(state['draft_seen_by']) - {'AN'}
        assert required <= set(state['retraction_notified_to']), 'A prior viewer was not notified'


def condition_fields(c):
    if 'field' in c: yield c['field']
    for kind in ['all', 'any']:
        for child in c.get(kind, []): yield from condition_fields(child)
    if 'not' in c: yield from condition_fields(c['not'])


def static_checks():
    assert set(scenes) == {'S00','S01','S02','S03','S04','S05','S06','S07','S08','S09_A','S09_B','S10','E_A','E_B','E_C'}
    ids = set()
    choice_count = 0
    text_count = 0
    for sid, scene in scenes.items():
        assert scene['scene_id'] == sid
        assert scene['background_id'] in assets
        assert all(a in assets for a in scene['asset_ids'])
        steps = {n['step_id'] for n in scene['steps']}
        assert len(steps) == len(scene['steps']) and scene['entry_step'] in steps
        lines = {l['text_id']: l for l in dialogue[sid]['lines']}
        assert len(lines) == len(dialogue[sid]['lines'])
        text_count += len(lines)
        used = set()
        for line in lines.values():
            assert line['speaker_id'] in speakers and line['text'].strip()
            assert '\ufffd' not in line['text']
        for node in scene['steps']:
            assert node['choices'], node['step_id']
            used.update(node['lines'])
            for c in node['choices']:
                assert c['choice_id'] not in ids
                ids.add(c['choice_id']); choice_count += 1
                assert set(condition_fields(c['when'])) <= set(initial)
                used.update(c['lines'])
                target = c['next']
                assert target == 'end' or target.startswith(('scene:', 'step:'))
                if target.startswith('scene:'): assert target[6:] in scenes
                if target.startswith('step:'): assert target[5:] in steps
                assert c['duration_minutes'] == sum(e['value'] for e in c['effects'] if e['op'] == 'advance_minutes')
                for e in c['effects']:
                    assert e['op'] in {'set','add_unique','advance_minutes','grant'}
                    if 'field' in e: assert e['field'] in initial
                    if e['op'] == 'grant':
                        assert e.get('asset_id', '') in assets or e.get('asset_field') in initial
                        assert e['source_event_id'] == c['choice_id']
                        assert e.get('scope') in {'yearbook_print','private_album','internal_review','supplement_delivery'} or e.get('scope_field')=='final_scope'
        assert used == set(lines), sid + ': Missing or unreferenced text'
    facts = read(STORY/'facts.json')['facts']
    assert len(facts) == 20 and len({f['fact_id'] for f in facts}) == 20
    assert assets['PHOTO_0443']['subjects'] == ['AN','HA','MINH']
    assert assets['PHOTO_S00_GOOD']['subjects'] == ['AN','NHI','HA','MINH']
    allowed = read(CONTENT/'Intents/S03/allowed-intents.json')
    pairs = {(i['id'], t) for i in allowed['intents'] for t in i['targets']}
    assert not {'PUBLISH','SET_CONSENT','SET_ENDING'} & {i['id'] for i in allowed['intents']}
    responses = read(CONTENT/'Intents/S03/responses.json')['entries']
    assert len({r['response_id'] for r in responses}) == len(responses)
    for r in responses:
        assert (r['intent'], r['target']) in pairs
        assert set(condition_fields(r['when'])) <= set(initial)
    for known in [False,True]:
        snapshot = copy.deepcopy(initial)
        snapshot['ha_shared_facts'] = ['F08'] if known else []
        matched = [r for r in responses if r['intent']=='ASK_QUOTE' and check(r['when'],snapshot)]
        assert len(matched) == 1
        assert matched[0]['response_id'] == ('QUOTE_KNOWN' if known else 'QUOTE_UNKNOWN')
    return choice_count, text_count


def route(name, overrides=None):
    overrides = overrides or {}
    state = copy.deepcopy(initial)
    sid = 'S00'; pos = scenes[sid]['entry_step']; visited = set(); selected=[]
    for _ in range(250):
        node = next(n for n in scenes[sid]['steps'] if n['step_id']==pos)
        available = [c for c in node['choices'] if check(c['when'],state)]
        assert available, (name, sid, pos, 'Dead end')
        wanted = overrides.get(pos)
        if wanted:
            c = next((c for c in available if c['choice_id']==wanted),None)
            assert c, (name, pos, wanted, 'Not available')
        else:
            c = next((c for c in available if c['choice_id'] not in visited),available[0])
        visited.add(c['choice_id']); selected.append(c['choice_id'])
        if c['choice_id']=='S09_A_CAPTURE':
            assert state['attendance']=='present' and state['courtyard_minute']<70
        if c['choice_id']=='S08_EXTEND': assert state['courtyard_minute']<60
        if c['choice_id'].startswith('S10_PUBLISH_'): validate_publication(state)
        apply(c['effects'],state)
        if c['next']=='end':
            assert state['finished'] and state['print_submitted'] and state['final_artifact_requested']
            if state['ending_id']=='E_C':
                assert state['supplement_delivered'] and state['later_status']=='accepted'
                if state['final_new_asset']:
                    assert rights_ok(state,state['final_new_asset'],'supplement_delivery')
            return state, selected
        if c['next'].startswith('scene:'):
            sid=c['next'][6:]; pos=scenes[sid]['entry_step']
            if sid=='E_C': assert state['later_status']=='accepted'
        else:
            pos=c['next'][5:]
    raise AssertionError((name,'Exceeded route limit'))


def run_checks():
    counts = static_checks()
    tests = [
      ('four',{},'E_A','four'),
      ('early_understanding',{'S05_TEMPLATE':'S05_TYPE_PAIRED','S04_SOURCE':'S04_TESTIMONY'},'E_A','four'),
      ('ha_handover',{'S06_TALK':'S06_BLAME','S06_PLACE':'S06_HANDOVER'},'E_A','four'),
      ('declined_three',{'S07_INVITE':'S07_ASK_AVAILABILITY'},'E_A','three'),
      ('pending_three',{'S07_INVITE':'S07_INVITE_SHORT'},'E_A','three'),
      ('no_new',{'S07_INVITE':'S07_ASK_AVAILABILITY','S09_B_DECIDE':'S09_B_NO_PHOTO'},'E_B','none'),
      ('three_private',{'S07_INVITE':'S07_INVITE_SHORT','S09_B_THREE_RIGHTS':'S09_B_THREE_PRIVATE'},'E_B','three'),
      ('four_not_printed',{'S09_A_RIGHTS':'S09_A_PRIVATE_ONLY'},'E_B','four'),
      ('leave_early',{'S09_A_CAMERA':'S09_A_REBUILD'},'E_A','three'),
      ('forced_pose_changed',{'S09_A_FRAME':'S09_A_OLD_POSE'},'E_A','four'),
      ('forced_pose_stopped',{'S09_A_FRAME':'S09_A_OLD_POSE','S09_A_REFUSE':'S09_A_STOP_FRAME'},'E_A','three'),
      ('tribute_retracted',{'S05_TEMPLATE':'S05_TYPE_TRIBUTE','S05_RECIPIENT':'S05_SEND_GROUP_TRIBUTE'},'E_A','four'),
      ('tribute_board_kept',{'S05_TEMPLATE':'S05_TYPE_TRIBUTE','S05_RECIPIENT':'S05_SEND_GROUP_TRIBUTE','S07_REACT':'S07_DISCUSS_AT_FIELD','S09_A_BOARD':'S09_A_LEAVE_BOARD'},'E_A','three'),
      ('pressure_absence',{'S05_TEMPLATE':'S05_TYPE_TRIBUTE','S07_REACT':'S07_INSIST'},'E_A','three'),
      ('later_old',{'S07_INVITE':'S07_ASK_AVAILABILITY','S09_B_DECIDE':'S09_B_LATER','S10_NEW':'S10_SUPPLEMENT'},'E_C','none'),
      ('later_new',{'S07_INVITE':'S07_ASK_AVAILABILITY','S09_B_DECIDE':'S09_B_LATER','S10_NEW':'S10_SUPPLEMENT','E_C_ARRIVE':'E_C_ASK_NEW'},'E_C','later'),
      ('later_new_private',{'S07_INVITE':'S07_INVITE_SHORT','S09_B_DECIDE':'S09_B_LATER','S10_NEW':'S10_SUPPLEMENT','E_C_ARRIVE':'E_C_ASK_NEW','E_C_PHOTO_RIGHTS':'E_C_LATER_PRIVATE'},'E_C','later'),
      ('later_pending',{'S07_INVITE':'S07_INVITE_SHORT','S09_B_DECIDE':'S09_B_LATER','S09_B_LATER':'S09_B_LATER_PENDING'},'E_B','none'),
      ('old_bad_01',{'S10_OLD':'S10_OLD_2'},'E_A','four'),
      ('old_bad_02',{'S10_OLD':'S10_OLD_3'},'E_A','four'),
      ('old_diagram',{'S10_OLD':'S10_OLD_4'},'E_A','four'),
      ('extended',{'S08_PRINT':'S08_EXTEND'},'E_A','four'),
    ]
    results=[]
    for name,overrides,ending,photo in tests:
        state,path=route(name,overrides)
        assert (state['ending_id'],state['photo_result'])==(ending,photo), (name,state['ending_id'],state['photo_result'])
        if name=='ha_handover': assert state['ha_support']=='handover_only'
        if name=='early_understanding': assert not state['saw_video'] and not state['tribute_board_prepared']
        if name=='tribute_retracted': assert {'HA','MINH'} <= set(state['draft_seen_by']) and state['tribute_board_removed']
        results.append({'case':name,'ending':ending,'photo':photo,'choices':len(path),'result':'PASS'})
    # Fail-closed conditions: invitation text, appearance and actual print grants are distinct.
    capture=next(c for n in scenes['S09_A']['steps'] for c in n['choices'] if c['choice_id']=='S09_A_CAPTURE')
    for attendance in ['absent','left']:
        snapshot=copy.deepcopy(initial); snapshot.update(attendance=attendance,nhi_wants_capture=True,equipment_ready='ready')
        assert not check(capture['when'],snapshot)
    base,_=route('permission_negative')
    for subject in ['NHI','HA','MINH']:
        invalid=copy.deepcopy(base)
        invalid['consent_grants']=[g for g in invalid['consent_grants'] if not (g['asset_id']==invalid['final_old_asset'] and g['subject_id']==subject and g['scope']=='yearbook_print')]
        try: validate_publication(invalid)
        except AssertionError: pass
        else: raise AssertionError('Published without '+subject+' old source grant')
    print(json.dumps({'result':'PASS','scenes':len(scenes),'choices':counts[0],'dialogue_entries':counts[1],
                      'route_cases':len(results),'negative_cases':5,'cases':results,
                      'limits':'Authoring checks only; no Unity, media, provider or human playtest verified.'},ensure_ascii=False,indent=2))


if __name__=='__main__':
    run_checks()
