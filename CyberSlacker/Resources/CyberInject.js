(function () {
    const selectors = ['.markdown-body', '.releaselog-content', 'article'];
    let content = null;
    for (let s of selectors) {
        content = document.querySelector(s);
        if (content) break;
    }
    let rawHtml = content ? content.innerHTML : "System Error: Missing Data";

    const stage = document.createElement('div');
    stage.id = 'cyber-stage';
    stage.innerHTML = `
        <div id='cyber-content'>
            <div class='markdown-body'>${rawHtml}</div>
        </div>
        <div id='shutter-t' class='shutter'></div>
        <div id='shutter-b' class='shutter'></div>
        <div id='shutter-line'></div>
    `;

    const style = document.createElement('style');
    style.innerHTML = `
        html, body { background: #0A0A0A !important; margin: 0; padding: 0; overflow: hidden !important; }
        #cyber-stage { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: #0A0A0A; z-index: 999999; }

        #cyber-stage::after {
            content: ""; position: absolute; top: 0; left: 0; width: 100%; height: 100%;
            pointer-events: none; z-index: 1000000; opacity: 0.3;
            background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.25) 50%),
                        linear-gradient(90deg, rgba(255, 0, 0, 0.03), rgba(0, 255, 0, 0.01), rgba(0, 0, 255, 0.03));
            background-size: 100% 3px, 3px 100%;
        }

        #cyber-content {
            padding: 10px 12px; height: 100vh; overflow-y: auto; box-sizing: border-box;
            opacity: 0; filter: blur(15px); transform: scale(1.05);
            transition: all 1s cubic-bezier(0.16, 1, 0.3, 1);
            --color-prettylights-syntax-keyword: #ff7b72;
            --color-prettylights-syntax-string: #a5d6ff;
        }

        .markdown-body h1, .markdown-body h2 {
            border: none !important; color: #00E5FF !important;
            font-size: 18px !important; letter-spacing: 2px !important;
            display: flex; align-items: center; margin: 30px 0 15px 0 !important;
        }
        .markdown-body h2::before {
            content: ""; margin-right: 12px; font-size: 20px;
            text-shadow: 0 0 10px #00E5FF;
        }

        .markdown-body ul li:has(.user-mention) {
            display: flex !important;
            align-items: center !important;
            border-left: 2px solid #333;
            padding: 6px 10px !important;
            white-space: nowrap !important;
            justify-content: space-between !important; 
            color: transparent !important;
            font-size: 0 !important;
            margin-bottom: 5px;

            * {
                color: #BBBBBB !important;
                font-size: 14px !important;
            }

            code {
               background: transparent !important;
               justify-content: space-between !important;
               font-weight: bold !important;
            }

            .user-mention {
                margin-left: 20px !important; background: rgba(0, 229, 255, 0.1) !important;
                color: #00E5FF !important; border: 1px solid rgba(0, 229, 255, 0.4) !important;
                padding: 1px 8px !important; border-radius: 4px !important;
                font-size: 11px !important; text-shadow: 0 0 5px #00E5FF;
                font-weight: bold;
                border-radius: 4px !important;
                text-shadow: 0 0 5px rgba(255, 166, 87, 0.5);
            }
        }

        .markdown-body {
            color: #D1D9E0 !important;
            font-family: 'Consolas', 'Segoe UI', monospace !important;
            letter-spacing: -0.2px !important;
            line-height: 1.4 !important;
        }

        .markdown-body a { color: #00E5FF !important; text-decoration: none !important; }
        .markdown-body a:hover { text-shadow: 0 0 8px #00E5FF; }

        .shutter { position: absolute; left: 0; width: 100%; height: 50.5%; background: #0A0A0A; z-index: 100; transition: transform 0.8s cubic-bezier(0.8, 0, 0.2, 1); }
        #shutter-t { top: 0; border-bottom: 2px solid #00E5FF; }
        #shutter-b { bottom: 0; border-top: 2px solid #00E5FF; }
        #shutter-line { position: absolute; top: 50%; left: 0; width: 100%; height: 1px; background: #00E5FF; z-index: 101; box-shadow: 0 0 15px #00E5FF; transition: all 0.5s ease 0.6s; }

        #cyber-stage.active #shutter-t { transform: translateY(-100%); }
        #cyber-stage.active #shutter-b { transform: translateY(100%); }
        #cyber-stage.active #shutter-line { opacity: 0; transform: scaleX(0); }
        #cyber-stage.active #cyber-content { opacity: 1; filter: blur(0px); transform: scale(1); }

        ::-webkit-scrollbar { width: 3px; }
        ::-webkit-scrollbar-thumb { background: #333; }
    `;

    document.head.appendChild(style);
    document.body.appendChild(stage);

    requestAnimationFrame(() => {
        setTimeout(() => { stage.classList.add('active'); }, 200);
    });
})();