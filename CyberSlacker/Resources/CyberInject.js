(function () {
    const selectors = ['.markdown-body', '.releaselog-content', 'article'];
    let content = null;
    for (let s of selectors) {
        content = document.querySelector(s);
        if (content) break;
    }
    const htmlContent = content ? content.innerHTML : "System Error: Content Missing";

    const stage = document.createElement('div');
    stage.id = 'cyber-stage';
    stage.innerHTML = `
        <div id='cyber-content'>
            <div class='markdown-body'>${htmlContent}</div>
        </div>
        <div id='shutter-t' class='shutter'></div>
        <div id='shutter-b' class='shutter'></div>
        <div id='shutter-line'></div>
    `;

    const style = document.createElement('style');
    style.innerHTML = `
        body > *:not(#cyber-stage) { display: none !important; }
        html, body { background: #1A1A1A !important; margin: 0; padding: 0; overflow: hidden !important; }
        
        #cyber-stage {
            position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
            background: #1A1A1A; z-index: 999999; overflow: hidden;
        }

        #cyber-content {
            padding: 30px; 
            height: 100vh; overflow-y: auto; box-sizing: border-box;
            opacity: 0.05;
            filter: blur(10px) brightness(0.5);
            transform: scale(0.95);
            will-change: opacity, filter, transform;
            transition: 
                opacity 1.2s cubic-bezier(0.2, 0, 0.2, 1),
                filter 1.2s cubic-bezier(0.2, 0, 0.2, 1),
                transform 1.2s cubic-bezier(0.2, 0, 0.2, 1);
        }

        .shutter {
            position: absolute; left: 0; width: 100%; height: 50.5%;
            background: #1A1A1A; z-index: 100;
            will-change: transform;
            transition: transform 1.0s cubic-bezier(0.8, 0, 0.2, 1);
        }
        #shutter-t { top: 0; border-bottom: 2px solid #00E5FF; }
        #shutter-b { bottom: 0; border-top: 2px solid #00E5FF; }

        #shutter-line {
            position: absolute; top: 50%; left: 0; width: 100%; height: 2px;
            background: #00E5FF; z-index: 101;
            box-shadow: 0 0 20px #00E5FF;
            transition: opacity 0.5s ease 0.5s, transform 0.5s ease 0.5s;
        }

        /* --- 激活状态 --- */
        #cyber-stage.active #shutter-t { transform: translateY(-100%); }
        #cyber-stage.active #shutter-b { transform: translateY(100%); }
        #cyber-stage.active #shutter-line { opacity: 0; transform: scaleX(0); }
        
        #cyber-stage.active #cyber-content { 
            opacity: 1; 
            filter: blur(0px) brightness(1);
            transform: scale(1);
        }

        /* 颜色控制 */
        .markdown-body, .markdown-body * { color: #BBBBBB !important; border-color: #333 !important; }
        .markdown-body h1, .markdown-body h2 { color: #00E5FF !important; }
        .markdown-body a { color: #00E5FF !important; text-decoration: none !important; }
        .markdown-body pre { background: #0D1117 !important; border: 1px solid #333 !important; }
        
        ::-webkit-scrollbar { width: 4px; }
        ::-webkit-scrollbar-thumb { background: #333; }
    `;

    document.head.appendChild(style);
    document.body.appendChild(stage);

    requestAnimationFrame(() => {
        setTimeout(() => {
            stage.classList.add('active');
        }, 200);
    });

})();