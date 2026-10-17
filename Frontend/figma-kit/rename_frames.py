# -*- coding: utf-8 -*-
"""Rename frames: select via canvas search, Ctrl+R, type the clean name."""
import sys, time, json
sys.path.insert(0, r"C:\Users\zzpha\Downloads\kiemtraa\Frontend\figma-kit")
from figma_cdp import CDP

c = CDP()


def select_by_search(q):
    c.key("Escape", "Escape"); time.sleep(0.3)
    b = c.js("""(()=>{const b=document.querySelector('[data-testid=canvas-search-icon-button]');
      const r=b.getBoundingClientRect();return {x:r.x+r.width/2,y:r.y+r.height/2}})()""")
    c.click(b["x"], b["y"]); time.sleep(0.9)
    c.key("a", "KeyA", modifiers=2)
    c.call("Input.insertText", text=q); time.sleep(1.8)
    hit = c.js("""(()=>{const els=[...document.querySelectorAll('[role=option],[data-testid*=search-result]')];
      const e=els.find(x=>x.innerText.trim().startsWith('%s'));
      if(!e) return null; const b=e.getBoundingClientRect();
      return {x:b.x+b.width/2,y:b.y+b.height/2,txt:e.innerText.trim()}})()""" % q)
    if not hit:
        return None
    c.click(hit["x"], hit["y"]); time.sleep(1.3)
    return hit["txt"]


def rename(new):
    c.key("r", "KeyR", modifiers=2); time.sleep(0.8)
    act = c.js("""(()=>{const a=document.activeElement;return a.tagName+'|'+(a.value!==undefined?a.value:'')})()""")
    if not act.startswith("INPUT"):
        return "no-input:" + act
    c.key("a", "KeyA", modifiers=2); time.sleep(0.15)
    c.call("Input.insertText", text=new); time.sleep(0.35)
    c.key("Enter", "Enter"); time.sleep(0.7)
    return "ok"


plan = json.load(open(sys.argv[1], encoding="utf-8"))
for it in plan["frames"]:
    txt = select_by_search(it["old"])
    if not txt:
        print("MISSING", it["old"]); continue
    print("%-6s %-34s -> %-32s %s" % (it["old"], txt, it["new"], rename(it["new"])))
    c.key("Escape", "Escape"); time.sleep(0.3)
