"""Quick look at a built character: renders front, 3/4, side, back and head with Workbench so proportions can be judged in seconds."""
import math
import os
import sys
import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import build_characters as bc

argv = sys.argv[sys.argv.index('--') + 1:]
out = argv[0]
names = argv[1:]
os.makedirs(out, exist_ok=True)
try:
    for name in names:
        bc.make_character(name, bc.PEOPLE[name], out)
        scene = bpy.context.scene
        scene.render.engine = 'BLENDER_WORKBENCH'
        scene.display.shading.color_type = 'VERTEX'
        scene.display.shading.light = 'STUDIO'
        scene.display.shading.show_cavity = False
        scene.display.shading.show_object_outline = True
        scene.render.resolution_x, scene.render.resolution_y = 560, 900
        h = bc.PEOPLE[name].get('H', 1.62)
        cam = bpy.data.objects.new('cam', bpy.data.cameras.new('cam'))
        scene.collection.objects.link(cam)
        scene.camera = cam
        cam.data.lens = 85
        for tag, ang, zoom, tz in (('front', 0, 1.0, .5), ('q', 35, 1.0, .5), ('side', 90, 1.0, .5), ('back', 180, 1.0, .5), ('head', 22, .33, .89)):
            a = math.radians(ang)
            d = 3.3 * h * zoom
            target = Vector((0, 0, h * tz))
            cam.location = target + Vector((math.sin(a) * d, -math.cos(a) * d, .1))
            cam.rotation_euler = (target - cam.location).to_track_quat('-Z', 'Y').to_euler()
            scene.render.filepath = os.path.join(out, f'{name}_{tag}.png')
            bpy.ops.render.render(write_still=True)
except Exception:
    import traceback
    with open(os.path.join(out, 'preview-error.log'), 'w') as f:
        traceback.print_exc(file=f)
with open(os.path.join(out, 'preview.done'), 'w') as f:
    f.write('ok')
