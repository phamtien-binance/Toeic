# -*- coding: utf-8 -*-
"""Collapse expanded layer rows, then position frames one by one."""
import sys, time, json
sys.path.insert(0, r"C:\Users\zzpha\Downloads\kiemtraa\Frontend\figma-kit")
from figma_cdp import CDP

c = CDP()


def rows():
    return c.js("""(()=>{const out=[];document.querySelectorAll('[data-testid=tree-grid-row-container]')
      .forEach(e=>{const b=e.getBoundingClientRect();
      if(b.y>50 && b.y<720) out.push({n:e.innerText.split(String.fromCharCode(10))[0],
      x:b.x+b.width/2,y:b.y+b.height/2,left:b.x,w:b.width})});return out})()""")


def scroll_top(top=0):
    c.js("""(()=>{const s=[...document.querySelectorAll('*')].filter(e=>e.scrollHeight>e.clientHeight+20&&e.clientHeight>100)[0];if(s)s.scrollTop=%d})()""" % top)
    time.sleep(0.35)


def collapse_all():
    """Click the disclosure arrow of any row that has one, top to bottom, three passes."""
    for _ in range(3):
        scroll_top(0)
        rs = rows()
        clicked = False
        for r in rs:
            # disclosure arrow sits at the left edge of the row
            if r["w"] > 0:
                c.click(r["left"] + 11, r["y"])
                time.sleep(0.25)
                clicked = True
                break
        if not clicked:
            break
    scroll_top(0)


def find(prefix):
    for top in [0, 120, 240, 360, 480, 640, 800]:
        scroll_top(top)
        for r in rows():
            if r["n"].startswith(prefix):
                return r
    return None


def set_input(aria, value):
    box = c.js("""(()=>{const i=document.querySelector('input[aria-label="%s"]')
      || [...document.querySelectorAll('input')].find(i=>i.getAttribute('aria-label')==='%s');
      if(!i) return null; const b=i.getBoundingClientRect(); return {x:b.x+b.width/2,y:b.y+b.height/2}})()""" % (aria, aria))
    if not box:
        return False
    c.click(box["x"], box["y"]); time.sleep(0.2)
    c.key("a", "KeyA", modifiers=2); time.sleep(0.1)
    c.call("Input.insertText", text=str(value)); time.sleep(0.25)
    c.key("Enter", "Enter"); time.sleep(0.4)
    return True


def read_pos():
    return c.js("""(()=>{const g=a=>{const i=document.querySelector('input[aria-label="'+a+'"]');return i?i.value:'?'};return g('X-position')+','+g('Y-position')})()""")


plan = json.load(open(sys.argv[1], encoding="utf-8"))
for it in plan["frames"]:
    collapse_all()
    r = find(it["old"])
    if not r:
        print("MISSING", it["old"]); continue
    c.click(r["x"], r["y"]); time.sleep(0.7)
    okx = set_input("X-position", it["x"])
    oky = set_input("Y-position", it["y"])
    got = read_pos()
    c.key("Escape", "Escape"); time.sleep(0.3)
    print("%-4s pos=%s want=%s,%s inputs=%s%s" % (it["old"], got, it["x"], it["y"], okx, oky))
