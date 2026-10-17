# -*- coding: utf-8 -*-
"""Position Figma frames by scrolling the (virtualised) layer list to find each row."""
import sys, time, json
sys.path.insert(0, r"C:\Users\zzpha\Downloads\kiemtraa\Frontend\figma-kit")
from figma_cdp import CDP

c = CDP()
SCROLLER = """(()=>{const s=[...document.querySelectorAll('*')].filter(e=>e.scrollHeight>e.clientHeight+20&&e.clientHeight>100)[0];return s||null})()"""


def rows():
    return c.js("""(()=>{const out=[];document.querySelectorAll('[data-testid=tree-grid-row-container]')
      .forEach(e=>{const b=e.getBoundingClientRect();
      if(b.y>60 && b.y<700) out.push({n:e.innerText.split(String.fromCharCode(10))[0],
      x:b.x+b.width/2,y:b.y+b.height/2})});return out})()""")


def scroll_top(top):
    c.js("""(()=>{const s=[...document.querySelectorAll('*')].filter(e=>e.scrollHeight>e.clientHeight+20&&e.clientHeight>100)[0];if(s)s.scrollTop=%d})()""" % top)
    time.sleep(0.5)


def find(prefix):
    for top in range(0, 2200, 160):
        scroll_top(top)
        for r in rows():
            if r["n"].startswith(prefix):
                return r
    return None


def set_input(aria, value):
    box = c.js("""(()=>{const i=document.querySelector('input[aria-label="%s"]');
      if(!i) return null; const b=i.getBoundingClientRect(); return {x:b.x+b.width/2,y:b.y+b.height/2}})()""" % aria)
    c.click(box["x"], box["y"]); time.sleep(0.2)
    c.key("a", "KeyA", modifiers=2); time.sleep(0.1)
    c.call("Input.insertText", text=str(value)); time.sleep(0.25)
    c.key("Enter", "Enter"); time.sleep(0.4)


def read_pos():
    return c.js("""(()=>{const g=a=>{const i=document.querySelector('input[aria-label="'+a+'"]');return i?i.value:null};return g('X-position')+','+g('Y-position')})()""")


def rename(new):
    c.key("r", "KeyR", modifiers=2); time.sleep(0.7)
    act = c.js("""(()=>{const a=document.activeElement;return a.tagName+'|'+(a.value!==undefined?a.value:'')})()""")
    if not act.startswith("INPUT"):
        return "skip(" + act + ")"
    c.key("a", "KeyA", modifiers=2)
    c.call("Input.insertText", text=new); time.sleep(0.3)
    c.key("Enter", "Enter"); time.sleep(0.6)
    return "renamed"


plan = json.load(open(sys.argv[1], encoding="utf-8"))
for it in plan["frames"]:
    r = find(it["old"])
    if not r:
        print("MISSING", it["old"]); continue
    c.click(r["x"], r["y"]); time.sleep(0.7)
    set_input("X-position", it["x"])
    set_input("Y-position", it["y"])
    got = read_pos()
    rn = rename(it["new"]) if it.get("new") else "-"
    c.click(1150, 700); time.sleep(0.3)   # empty canvas (deselect)
    print("%-8s %-30s pos=%s want=%s,%s %s" % (it["old"], r["n"], got, it["x"], it["y"], rn))
