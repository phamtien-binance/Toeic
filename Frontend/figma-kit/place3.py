# -*- coding: utf-8 -*-
"""Select a frame through the canvas search box, then set its X/Y via the right panel."""
import sys, time, json
sys.path.insert(0, r"C:\Users\zzpha\Downloads\kiemtraa\Frontend\figma-kit")
from figma_cdp import CDP

c = CDP()

SEARCH_BTN = """(()=>{const b=document.querySelector('[data-testid=canvas-search-icon-button]');
  const r=b.getBoundingClientRect();return {x:r.x+r.width/2,y:r.y+r.height/2}})()"""


def select_by_search(q):
    c.key("Escape", "Escape"); time.sleep(0.3)
    b = c.js(SEARCH_BTN)
    c.click(b["x"], b["y"]); time.sleep(0.9)
    c.key("a", "KeyA", modifiers=2)
    c.call("Input.insertText", text=q); time.sleep(1.4)
    hit = c.js("""(()=>{const els=[...document.querySelectorAll('[role=option],[data-testid*=search-result]')];
      const e=els.find(x=>x.innerText.trim().startsWith('%s'));
      if(!e) return null; const b=e.getBoundingClientRect();
      return {x:b.x+b.width/2,y:b.y+b.height/2,txt:e.innerText.trim()}})()""" % q)
    if not hit:
        return None
    c.click(hit["x"], hit["y"]); time.sleep(1.4)
    return hit["txt"]


def set_input(aria, value):
    box = c.js("""(()=>{const i=document.querySelector('input[aria-label="%s"]');
      if(!i) return null; const b=i.getBoundingClientRect(); return {x:b.x+b.width/2,y:b.y+b.height/2}})()""" % aria)
    if not box:
        return False
    c.click(box["x"], box["y"]); time.sleep(0.2)
    c.key("a", "KeyA", modifiers=2); time.sleep(0.1)
    c.call("Input.insertText", text=str(value)); time.sleep(0.25)
    c.key("Enter", "Enter"); time.sleep(0.45)
    return True


def read_pos():
    return c.js("""(()=>{const g=a=>{const i=document.querySelector('input[aria-label="'+a+'"]');return i?i.value:'?'};return g('X-position')+','+g('Y-position')})()""")


plan = json.load(open(sys.argv[1], encoding="utf-8"))
for it in plan["frames"]:
    txt = select_by_search(it["old"])
    if not txt:
        print("MISSING", it["old"]); continue
    set_input("X-position", it["x"])
    set_input("Y-position", it["y"])
    print("%-4s %-32s pos=%s want=%s,%s" % (it["old"], txt, read_pos(), it["x"], it["y"]))
    c.key("Escape", "Escape"); time.sleep(0.3)
