# -*- coding: utf-8 -*-
"""Minimal CDP client for the user-visible Chrome on :9222 (Figma driving)."""
import json, time, urllib.request
import websocket


class CDP:
    def __init__(self, port=9222, target_filter=None, retries=30):
        self.port = port
        self.id = 0
        self.ws = None
        self.target = None
        for _ in range(retries):
            target = self._pick(target_filter)
            if target:
                try:
                    self.ws = websocket.create_connection(
                        target["webSocketDebuggerUrl"], timeout=30, suppress_origin=True)
                    self.target = target
                    return
                except Exception:
                    time.sleep(0.4)
            time.sleep(0.5)
        raise RuntimeError("no debuggable page target on port %d" % port)

    def _pick(self, filt):
        try:
            pages = json.load(urllib.request.urlopen("http://127.0.0.1:%d/json/list" % self.port))
        except Exception:
            return None
        pages = [p for p in pages if p.get("type") == "page"]
        if not pages:
            return None
        if filt:
            for p in pages:
                if filt in (p.get("url") or ""):
                    return p
            return None
        return pages[0]

    def call(self, method, **params):
        self.id += 1
        mid = self.id
        self.ws.send(json.dumps({"id": mid, "method": method, "params": params}))
        while True:
            msg = json.loads(self.ws.recv())
            if msg.get("id") == mid:
                if "error" in msg:
                    raise RuntimeError("%s -> %s" % (method, msg["error"]))
                return msg.get("result", {})

    def js(self, expr, await_promise=False):
        r = self.call("Runtime.evaluate", expression=expr, returnByValue=True,
                      awaitPromise=await_promise, userGesture=True)
        if r.get("exceptionDetails"):
            raise RuntimeError(r["exceptionDetails"].get("text") or r["exceptionDetails"])
        return r.get("result", {}).get("value")

    def nav(self, url, wait=3.0):
        self.call("Page.enable")
        self.call("Page.navigate", url=url)
        time.sleep(wait)

    def front(self):
        try:
            self.call("Page.bringToFront")
            wid = self.call("Browser.getWindowForTarget", targetId=self.target["id"])["windowId"]
            self.call("Browser.setWindowBounds", windowId=wid,
                      bounds={"windowState": "normal", "width": 1512, "height": 950})
            self.call("Browser.setWindowBounds", windowId=wid, bounds={"windowState": "maximized"})
        except Exception as e:
            print("bringToFront failed:", e)

    def key(self, key, code=None, modifiers=0, kind="keyDown"):
        code = code or ("Key" + key.upper() if len(key) == 1 and key.isalpha() else key)
        vk = {"Escape": 27, "Enter": 13, "Tab": 9, "Backspace": 8, "Delete": 46, "Shift": 16,
              "Control": 17}.get(key, ord(key[0]) if len(key) == 1 else 0)
        p = {"type": kind, "key": key, "code": code, "windowsVirtualKeyCode": vk,
             "nativeVirtualKeyCode": vk, "modifiers": int(modifiers)}
        self.call("Input.dispatchKeyEvent", **p)

    def type_text(self, text):
        for ch in text:
            self.call("Input.dispatchKeyEvent", type="keyDown", text=ch)
            self.call("Input.dispatchKeyEvent", type="keyUp", text=ch)

    def click(self, x, y, click_count=1):
        self.call("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", clickCount=click_count)
        self.call("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", clickCount=click_count)

    def close(self):
        try:
            self.ws.close()
        except Exception:
            pass
