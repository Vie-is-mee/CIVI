from collections import Counter
import hashlib
import json
from pathlib import Path
import tempfile
import unittest
from dialogue_content import DIALOGUE, ROOT, parse
from review_dialogue_files import NODES, validate
from compile_chapter_dialogue import pages


class DialogueContentTests(unittest.TestCase):
    def setUp(self):
        self.content=json.loads((ROOT/'Game/Assets/_Civi/Resources/Chapter1/chapter.json').read_text(encoding='utf-8'))

    def test_seventeen_files_and_twelve_routes(self):
        report=validate()
        self.assertEqual(len(report['files']),17)
        self.assertEqual(len(report['routes']),12)
        for route in report['routes']:
            self.assertEqual('C10' in route, 'C9C' in route)

    def test_metadata_and_endings_are_not_spoken(self):
        for name in NODES.values():
            _,cues,errors=parse(DIALOGUE/name)
            self.assertFalse(errors)
            self.assertTrue(cues)
            self.assertTrue(all(c.speaker not in ['Vị trí','Lựa chọn','Cấu trúc'] for c in cues))
            self.assertTrue(all('Hết Chương' not in c.text and 'civi-route:' not in c.text for c in cues))

    def test_ending_c_subheadings_are_supported(self):
        scenes,cues,errors=parse(DIALOGUE/'Ending_C_Ban_Duyet.md')
        self.assertEqual(len(scenes),6)
        self.assertTrue(all(c.scene>0 for c in cues))

    def test_source_provenance_and_every_word_preserved(self):
        source=DIALOGUE/'Chuong_1_Ban_Duyet.md'
        self.assertEqual(self.content['dialogueSourceHash'],hashlib.sha256(source.read_bytes()).hexdigest())
        scenes,cues,_=parse(source)
        self.assertEqual(len(cues),self.content['sourceCueCount'])
        counts=Counter()
        used=[]
        for cue in cues:
            counts[cue.scene]+=1
            prefix=f"C1.S{cue.scene:02}.{counts[cue.scene]:03}.p"
            indices=[i for i,c in enumerate(self.content['cueIds']) if c.startswith(prefix)]
            self.assertTrue(indices)
            self.assertEqual(' '.join(self.content['paragraphs'][i] for i in indices),' '.join(cue.text.split()))
            self.assertTrue(all(self.content['sourceLines'][i]==cue.line and self.content['speakers'][i]==cue.speaker for i in indices))
            used.extend(indices)
        self.assertEqual(used,list(range(len(self.content['paragraphs']))))

    def test_action_gates_do_not_jump_over_dialogue(self):
        data=self.content
        order=data['intro']+[i for s in data['steps'] for key in ['before','after'] for i in s[key]]
        self.assertEqual(order,list(range(len(data['paragraphs']))))
        self.assertEqual(len(data['steps']),9)
        self.assertEqual(sum(s['action']!='none' for s in data['steps']),5)

    def test_page_breaks_and_role_separation(self):
        data=self.content
        self.assertLessEqual(max(map(len,data['paragraphs'])),360)
        self.assertIn('Người hướng dẫn thực tập',data['speakers'])
        self.assertIn('Hướng dẫn viên',data['speakers'])
        self.assertTrue(all(data['portraits'][i]==-1 for i,s in enumerate(data['speakers']) if s not in ['Mai','Linh','Vinh','Mẹ Mai']))
        self.assertEqual(' '.join(pages('Một câu. '*200)).split(),('Một câu. '*200).split())

    def test_malformed_speaker_fails_lint(self):
        directory=ROOT/'artifacts/dialogue-review/test-fixtures'
        directory.mkdir(parents=True,exist_ok=True)
        path=directory/'malformed.md'
        path.write_text('# Test\n## Cảnh 1 — Test\n**Mai: lời thiếu dấu đóng\n',encoding='utf-8')
        self.assertTrue(parse(path)[2])


if __name__=='__main__': unittest.main()
