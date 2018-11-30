/*
	maxwell.z for vesystem.
*/

//events for tabs
chrome.tabs.onUpdated.addListener(
    function (tabId, changeInfo) {
    if (changeInfo.status == "loading") {
        var id = "tabid" + tabId;
    }
    
});

chrome.tabs.onRemoved.addListener(function (tabId) {
    var id = "tabid" + tabId;
	
});

chrome.tabs.onMoved.addListener(function (tabId,moveInfo) {
     
});

chrome.tabs.onSelectionChanged.addListener(function (tabId, selectInfo) {

});

//post url到本地服务
function xmlhttprequest_post(url) {
    var xhr = new XMLHttpRequest();
    xhr.timeout = 5000;
    var formData = new FormData();
    formData.append('url', url);
    xhr.open('POST', 'http://localhost:8233/');
    xhr.send(formData);
   
    //delete xhr;
}

function onRequest(request, sender, callback) {
    xmlhttprequest_post(request.result);
};

function NotifyStart() {
    xmlhttprequest_post("WebStart");
}
// Wire up the listener.
chrome.extension.onRequest.addListener(onRequest);
//通知开始-浏览器启动调用
NotifyStart();
 