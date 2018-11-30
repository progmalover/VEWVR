/*
maxwell.z for vesystem.
*/

//track xtree structs,return video node if it is exist
var _YOUKU_ = 0, _IQIYI_ = 1, _SOHU_=2,_QQ_=3,_HAO123_=4;
var _White_sheet = ["youku", "iqiyi", "sohu", "qq","hao123"];
var _player_tags = [{ "_id": "module_basic_player" }, { "_id": "ykPlayer" }, { "_class": "youku-player" },
    { "_id": "tenvideo_player" }, { "_id": "playContainer" }, { "_class": "yk-trigger-layer" }, { "_class": "mod-play1er" },
    { "_class": "mod-play1er" }, {"_class":"video-container"}];

var exportStr = "<button  class=\"redirectBtn\" style=\"text-decoration: underline; color: blue;\"><视频重定向到终端播放></button>";

function createOnVideoDirectClickElement() {
    var elms = document.createElement("div");
    elms.innerHTML = exportStr;
    elms.firstChild.onclick = onVideoRedirectClick;
    return elms;
}

function onVideoRedirectClick() {
    chrome.extension.sendRequest({ result: document.URL });
}
//检查页面是否包含视频
function checkVideoNode() {

    //detect tags
    for (var i = 0; i < _player_tags.length; i++) {
        var val = _player_tags[i]["_id"];
        if (val != undefined) {
            if (document.getElementById(val) != undefined)
                return true;
        }
        val = _player_tags[i]["_class"];
        if (val != undefined) {
            var elms = document.getElementsByClassName(val);
            if (elms != undefined && elms.length > 0)
                return true;
        }
    }

    //检查h5
    var v = document.getElementsByTagName("video");
    if (v != undefined && v.length > 0) {
        return true;
    }

    //检查flash视频
  
    f = document.getElementsByTagName("EMBED");
    if (f.length <= 0)
        f = document.getElementsByTagName("OBJECT");
    if (f != undefined && f.length > 0) {
        //alert("found flash!");
        return true;
    }
 
    return false;
}

//停止视频播放，并试图删除播放器
function deleteVideoNodes(elm) {

    for (var i = 0; i < elm.length; i++) {
        var it = elm[i];
        var parent = it.parentNode;
       
        it.removeAttribute('src'); // empty source
        it.baseURI = "";
        it.disableRemotePlayback = true;
        it.cookie = "";
        it.defaultMuted = true;
        it.removeAttribute('preload'); // empty source
        it.muted = true;
        it.volume = 0;
        it.style.visibility = "hidden";

        it.pause();
        delete it;
        parent.innerHTML = "<div>";
        parent.appendChild(createOnVideoDirectClickElement());
    }
}
//停止视频播放，并试图删除播放器
function deleteVideoNode(elm) {

        var parent = elm.parentNode;
        //it.stop();
        elm.removeAttribute('src'); // empty source
        elm.baseURI = "";
        elm.disableRemotePlayback = true;
        elm.cookie = "";
        elm.defaultMuted = true;
        elm.removeAttribute('preload'); // empty source
        elm.muted = true;
        elm.volume = 0;
        elm.style.visibility = "hidden";

        elm.pause();
        delete elm;
        return parent;
}
 
var pageProcessor = {
   
    doWebCheck:function () {
        var curIndex = -1;
        for (var i = 0; i < _White_sheet.length; i++) {
            if (document.URL.indexOf(_White_sheet[i]) != -1) {
                curIndex = i;
                break;
            }
        }
        return curIndex;
    },

    doProcess:function (webId) {
        //判断网站
        var bEnable = checkVideoNode();

        //页面包含视频
        if (bEnable) {

            switch (webId) {

                case _YOUKU_: {
                    document.elmyk = document.getElementsByTagName("video");
                    if (document.elmyk != undefined && document.elmyk.length > 0) {
                        deleteVideoNodes(document.elmyk);

                    }
                    //搜索播放器node,直接改内容
                    var player = document.getElementById("module_basic_player");
                    if (player != undefined) {
                        player.innerHTML = "<div>";
                        player.appendChild(createOnVideoDirectClickElement());
                        //createOnVideoDirectClickHandle(player.parentElement);
                    }

                    chrome.extension.sendRequest({ result: document.URL });
                }
                    break;
                case _IQIYI_: {
                    document.elmiqi = document.getElementsByTagName("video");
                    if (document.elmiqi != undefined && document.elmiqi.length > 0) {
                        var repeat = function () {
                            deleteVideoNodes(document.elmiqi);
                        }
                        setTimeout(repeat, 200);
                        chrome.extension.sendRequest({ result: document.URL });
                    } else {

                        var elmiqis = document.getElementsByClassName("video-container");
                        for (i = 0; i < elmiqis.length; i++) {
                            var elmiqi = elmiqis[i];
                            elmiqi.innerHTML = "<div>";
                            elmiqi.appendChild(createOnVideoDirectClickElement());
                        }

                    }

                }
                    break;
                case _SOHU_: {
                    document.elmsu = document.getElementsByTagName("video");
                    if (document.elmsu != undefined && document.elmsu.length > 0) {
                        deleteVideoNodes(document.elmsu);
                    }

                    document.elmsu = document.getElementById("sohuplayer");
                    if (document.elmsu != undefined) {
                        document.elmsu.innerHTML = "<div>";
                        document.elmsu.appendChild(createOnVideoDirectClickElement());
                        //createOnVideoDirectClickHandle(document.elmsu.parentNode);
                    }

                    document.elmsu = document.getElementsByTagName("EMBED");
                    for (i = 0; i < document.elmsu.length; i++) {
                        document.elmsu[i].innerHTML = "<div>";
                        document.elmsu[i].appendChild(createOnVideoDirectClickElement());
                        //createOnVideoDirectClickHandle(document.elmsu[i].parentNode);
                    }
                    chrome.extension.sendRequest({ result: document.URL });

                }
                    break;
                case _QQ_: {
                    document.elmqq = document.getElementsByTagName("video");
                    if (document.elmqq != undefined && document.elmqq.length > 0) {
                        deleteVideoNodes(document.elmqq);
                    }

                    document.elmqq = document.getElementById("tenvideo_player");
                    if (document.elmqq != undefined) {
                        document.elmqq.innerHTML = "<div>";
                        document.elmqq.parentNode.appendChild(createOnVideoDirectClickElement());
                       // createOnVideoDirectClickHandle(document.elmqq.parentNode);
                    }
                    chrome.extension.sendRequest({ result: document.URL });

                }
                    break;
                case _HAO123_: {
                    document.elmhao = document.getElementById("playContainer");
                    if (document.elmhao != undefined) {
                        document.elmhao.innerHTML = "<div>";
                        document.elmhao.appendChild(createOnVideoDirectClickElement());
                       // createOnVideoDirectClickHandle(document.elmhao.parentNode);
                    }
                    chrome.extension.sendRequest({ result: document.URL });
                }
                    break;

                default: {
                    for (var i = 0; i < _player_tags.length; i++) {
                        var val = _player_tags[i]["_id"];
                        if (val != undefined) {
                            elm = document.getElementById(val);
                            if (elm != undefined) {
                                elm.innerHTML = "<div>";
                                elm.appendChild(createOnVideoDirectClickElement());
                                break;
                            }
                        }
                        val = _player_tags[i]["_class"];
                        if (val != undefined) {
                            var elms = document.getElementsByClassName(val);
                            for (i = 0; i < elms.length; i++) {
                                var elm = elms[i];
                                elm.innerHTML = "<div>";
                                elm.appendChild(createOnVideoDirectClickElement());
                            }
                        }
                    }

                    document.elm = document.getElementsByTagName("video");
                    if (document.elm != undefined && document.elm.length > 0) {
                        deleteVideoNodes(document.elm);
                    }

                    document.elm = document.getElementsByTagName("EMBED");
                    if (document.elm.length <= 0) {
                        f = document.getElementsByTagName("OBJECT");
                    }
                    for (i = 0; i < document.elm.length; i++) {
                        document.elm[i].innerHTML = "<div>";
                        document.elm[i].appendChild(createOnVideoDirectClickElement());
                        //createOnVideoDirectClickHandle(document.elm[i].parentNode);
                    }
                    chrome.extension.sendRequest({ result: document.URL });
                }
                    break;
            }

        }
    }
    
}

//与background消息交互
chrome.extension.onRequest.addListener(function (request, sender, sendResponse) {
    //sendResponse("content chrome.extension.onRequest callack response");
});

 
//安装click事件处理
function InstallPageListener(func) {
    var aa = document.getElementsByTagName("a");
    for (i = 0; i < aa.length; i++) {
        aa[i].addEventListener("click", func);
    }
}

//页面视频处理
function doPageProcess() {
    //    //say hello before page process 
    chrome.extension.sendRequest({ result: 'hello' });

    var webId = pageProcessor.doWebCheck();
    pageProcessor.doProcess(webId);
}

//页面点击hook,处理可能的弹窗式视频
function onPageClick() {
    // alert("onPageClick");
    setTimeout(function () {
        doPageProcess();
    },1000);
}

InstallPageListener(onPageClick);
doPageProcess();

 