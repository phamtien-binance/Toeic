# -*- coding: utf-8 -*-
"""Arrange imported frames into a grid + strip Figma's ' 1' suffix from names."""
import sys, time, json
sys.path.insert(0, r"C:\Users\zzpha\Downloads\kiemtraa\Frontend\figma-kit")
from figma_cdp import CDP

c = CDP()


def rows():
    return c.js("""(()=>[...document.querySelectorAll('[data-testid=tree-grid-row-container]')]
      .map(e=>{const b=e.getBoundingClientRect();return {n:e.innerText.split(String.fromCharCode(10))[0],
      x:b.x+b.width/2,y:b.y+b.height/2}}))()""")


def select(prefix):
    for r in rows():
        if r["n"].startswith(prefix):
            c.click(r["x"], r["y"])
            time.sleep(0.6)
            return r["n"]
    raise RuntimeError("row not found: " + prefix)


def set_input(aria, value):
    box = c.js("""(()=>{const i=document.querySelector('input[aria-label="%s"]');
      if(!i) return null; const b=i.getBoundingClientRect(); return {x:b.x+b.width/2,y:b.y+b.height/2}})()""" % aria)
    if not box:
        raise RuntimeError("input not found: " + aria)
    c.click(box["x"], box["y"])
    time.sleep(0.25)
    c.key("a", "KeyA", modifiers=2)              # Ctrl+A
    time.sleep(0.15)
    c.call("Input.insertText", text=str(value))
    time.sleep(0.3)
    c.key("Enter", "Enter")
    time.sleep(0.5)


def rename(new_name):
    c.key("r", "KeyR", modifiers=2)              # Ctrl+R
    time.sleep(0.8)
    act = c.js("""(()=>{const a=document.activeElement;return a? (a.tagName+'|'+(a.value!==undefined?a.value:'')) : 'none'})()""")
    if not act.startswith("INPUT"):
        return "no rename input: " + act
    c.key("a", "KeyA", modifiers=2)
    c.call("Input.insertText", text=new_name)
    time.sleep(0.4)
    c.key("Enter", "Enter")
    time.sleep(0.8)
    return "ok"


def canvas_click():
    c.click(880, 620)   # empty canvas area (bottom middle)
    time.sleep(0.4)


PLAN = json.load(open(sys.argv[1], encoding="utf-8"))
for item in PLAN["frames"]:
    cur = select(item["old"])
    set_input("X-position", item["x"])
    set_input("Y-position", item["y"])
    r = rename(item["new"])
    canvas_click()
    print("%-34s -> %-30s (%s,%s) rename:%s" % (cur, item["new"], item["x"], item["y"], r))

time.sleep(1)
print("final rows:")
for r in rows():
    print("  ", r["n"])
