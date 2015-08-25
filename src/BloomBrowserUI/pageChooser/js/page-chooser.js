/// <reference path="../../bookEdit/js/getIframeChannel.ts" />
/// <reference path="../../lib/localizationManager/localizationManager.ts" />
window.addEventListener("message", process_EditFrame_Message, false);
function process_EditFrame_Message(event) {
    var params = event.data.split("\n");
    switch (params[0]) {
        case "Data":
            var pageChooser = new PageChooser(params[1]);
            pageChooser.loadInstalledCollections();
            return;
        default:
    }
}
// this version of the test string may be useful later testing more than one template collection
//var JSONTestString = "[{ \"templateBookUrl\": \"../../../DistFiles/factoryCollections/Templates/Basic Book/Basic Book.htm\" }, { \"templateBookUrl\": \"../../../DistFiles/factoryCollections/Templates/Basic Book/Basic Book.htm\" }]";
// no longer using test string, but let's keep it around as documentation of what PageChooser's ctor is expecting
//var JSONTestString = "[{ \"templateBookUrl\": \"bloom/localhost/C$/BloomDesktop/DistFiles/factoryCollections/Templates/Basic Book/Basic Book.htm\" }]";
var PageChooser = (function () {
    function PageChooser(templateBookUrls) {
        if (templateBookUrls) {
            this._templateBookUrls = templateBookUrls;
        }
        else {
            console.log("Expected url in PageChooser ctor!");
        }
        this._selectedGridItem = undefined;
        this._indexOfPageToSelect = 0;
        this._scrollTopOfTheScrollingDiv = 0;
    }
    PageChooser.prototype.thumbnailClickHandler = function (clickedDiv, evt) {
        // 'div' is an .invisibleThumbCover
        // Select new thumbnail
        var newsel = this.findProperElement(clickedDiv, evt);
        if (newsel == null)
            return;
        // Mark any previously selected thumbnail as no longer selected
        if (this._selectedGridItem != undefined) {
            $(this._selectedGridItem).removeClass("ui-selected");
        }
        this._selectedGridItem = newsel;
        $(this._selectedGridItem).addClass("ui-selected");
        // Display large preview
        var caption = $('#previewCaption');
        var defaultCaptionText = $(".gridItemCaption", this._selectedGridItem).text();
        this.setLocalizedText(caption, 'TemplateBooks.PageLabel.', defaultCaptionText);
        caption.attr("style", "display: block;");
        $("#preview").attr("src", $(this._selectedGridItem).find("img").first().attr("src"));
        this.setLocalizedText($('#previewDescriptionText'), 'TemplateBooks.PageDescription.', $(".pageDescription", this._selectedGridItem).text(), defaultCaptionText);
    }; // thumbnailClickHandler
    // There's a bug deep in javascript that doesn't take into account the scrolling
    // of a div element before something inside it is clicked on.  The following code
    // detects whether the scrolling has changed since the last mouse click, and if so,
    // searches for the item which should have matched.  For the initial bug report,
    // see https://silbloom.myjetbrains.com/youtrack/issue/BL-2623.
    // Note that the offset().top values returned by jquery properly take into account
    // the scrollTop of the scrolling parent div.  Which makes me think the bug may be
    // below the jquery level!?
    PageChooser.prototype.findProperElement = function (clickedDiv, evt) {
        var gridItem = $(clickedDiv).parent();
        if (evt) {
            var currentScrollTop = this._scrollingDiv.scrollTop();
            if (currentScrollTop !== this._scrollTopOfTheScrollingDiv) {
                // The scrolling position has changed, so we need to explicitly search
                // for the proper object.
                var y = evt["clientY"]; // retrieve the original click position
                var x = evt["clientX"];
                var container = $(clickedDiv).parent().parent();
                var childs = $(container).children();
                for (var i = 0; i < childs.length; ++i) {
                    var child = childs.eq(i);
                    var top = child.offset().top;
                    var bottom = top + child.height();
                    var left = child.offset().left;
                    var right = left + child.width();
                    if (top <= y && y <= bottom && left <= x && x <= right) {
                        // Remember the new scroll position and return the proper object.
                        this._scrollTopOfTheScrollingDiv = currentScrollTop;
                        return child;
                    }
                }
                // We couldn't find the proper object, so don't do anything.  The user
                // apparently clicked on a visually empty spot that got misidentified.
                return null;
            }
        }
        return gridItem;
    };
    // Set the text of the given element to the appropriate localization of defaultText
    // (or to defaultText, if no localization is available).
    // If defaultText is empty, set the element text to empty.
    // The localization ID to look up is made by concatenating the supplied prefix and the id
    // parameter, which defaults to the defaultText since we often use the English text of a
    // label as the last part of its ID.
    PageChooser.prototype.setLocalizedText = function (elt, idPrefix, defaultText, id) {
        if (id === void 0) { id = defaultText; }
        if (defaultText) {
            localizationManager.asyncGetText(idPrefix + id, defaultText)
                .done(function (translation) {
                elt.text(translation);
            });
        }
        else {
            elt.text("");
        }
    };
    PageChooser.prototype.addPageClickHandler = function () {
        if (this._selectedGridItem == undefined || this._templateBookUrls == undefined)
            return;
        this.fireCSharpEvent("setModalStateEvent", "false");
        var id = this._selectedGridItem.attr("data-pageId");
        this.fireCSharpEvent("addPage", id);
    }; // addPageClickHandler
    PageChooser.prototype.loadInstalledCollections = function () {
        var _this = this;
        // Save a reference to the scrolling div that contains the various page items.
        this._scrollingDiv = $(".gridItemDisplay", document);
        // Originally (now maybe YAGNI) the dialog handled more than one collection of template pages.
        // Right now it only handles one, so the cloning of stub html is perhaps unnecessary,
        // but I've left it in case we need it later.
        // Save html sections that will get cloned later
        // there should only be one 'collection' at this point; a stub with one default template page
        var collectionHtml = $(".collection", document).first().clone();
        // there should only be the one default 'gridItem' at this point
        var gridItemHtml = $(".gridItem", collectionHtml).first().clone();
        var collectionUrls;
        try {
            collectionUrls = $.parseJSON(this._templateBookUrls);
        }
        catch (e) {
            console.log("Received bad template url: " + e);
            return;
        }
        var pageChooser = this;
        if ($(collectionUrls).length > 0) {
            // Remove original stub section
            $(".outerCollectionContainer", document).empty();
            $.each(collectionUrls, function (index) {
                //console.log('  ' + (index + 1) + ' loading... ' + this['templateBookUrl'] );
                var collectionLastPageAdded = this["lastPageAdded"];
                pageChooser.loadCollection(this["templateBookFolderUrl"], this["templateBookUrl"], collectionHtml, gridItemHtml, collectionLastPageAdded);
            });
        }
        $("#addPageButton", document).button().click(function () {
            _this.addPageClickHandler();
        });
        var pageButton = $("#addPageButton", document);
        localizationManager.asyncGetText('AddPageDialog.AddPageButton', 'Add This Page')
            .done(function (translation) {
            pageButton.attr('value', translation);
        });
    }; // LoadInstalledCollections
    PageChooser.prototype.loadCollection = function (pageFolderUrl, pageUrl, collectionHTML, gridItemHTML, lastPageAdded) {
        var _this = this;
        var request = $.get(pageUrl);
        request.done(function (pageData) {
            var dataBookArray = $("div[data-book='bookTitle']", pageData);
            var collectionTitle = $(dataBookArray.first()).text();
            // Add title and container to dialog
            var collectionToAdd = $(collectionHTML).clone();
            _this.setLocalizedText($(collectionToAdd).find(".collectionCaption"), 'TemplateBooks.BookName.', collectionTitle);
            $(".outerCollectionContainer", document).append(collectionToAdd);
            // Grab all pages in this collection
            // N.B. normal selector syntax or .find() WON'T work here because pageData is not yet part of the DOM!
            var pages = $(pageData).filter(".bloom-page[id]");
            _this._indexOfPageToSelect = _this.loadPagesFromCollection(collectionToAdd, pages, gridItemHTML, pageFolderUrl, pageUrl, lastPageAdded);
            _this.thumbnailClickHandler($(".invisibleThumbCover").eq(_this._indexOfPageToSelect), null);
        }, "html");
        request.fail(function (jqXHR, textStatus, errorThrown) {
            console.log("There was a problem reading: " + pageUrl + " see documentation on : " +
                jqXHR.status + " " + textStatus + " " + errorThrown);
        });
    }; // LoadCollection
    PageChooser.prototype.loadPagesFromCollection = function (currentCollection, pageArray, gridItemTemplate, pageFolderUrl, pageUrl, lastPageAdded) {
        var _this = this;
        if ($(pageArray).length < 1) {
            return 0;
        }
        // Remove default template page
        $(".innerCollectionContainer", currentCollection).empty();
        var indexToSelect = 0;
        // insert a template page for each page with the correct #id on the url
        $(pageArray).each(function (index, div) {
            if ($(div).attr("data-page") === "singleton")
                return; // skip this one
            var currentGridItemHtml = $(gridItemTemplate).clone();
            var currentId = $(div).attr("id");
            $(currentGridItemHtml).attr("data-pageId", currentId);
            if (currentId === lastPageAdded)
                indexToSelect = index;
            var pageDescription = $(".pageDescription", div).first().text();
            $(".pageDescription", currentGridItemHtml).first().text(pageDescription);
            var pageLabel = $(".pageLabel", div).first().text().trim();
            $(".gridItemCaption", currentGridItemHtml).first().text(pageLabel);
            // any changes to how we tweak the page label to get a file name
            // must also be made in EnhancedImageServer.FindOrGenerateImage().
            pageLabel = pageLabel.replace("&", "+"); //ampersands don't work in the svg file names, so we use "+" instead
            // gensvg is a 'magic' extension which the Bloom fileserver understands. See EnhancedImageServer.FindOrGenerateImage.
            $("img", currentGridItemHtml).attr("src", pageFolderUrl + "/template" + "/" + pageLabel + ".gensvg");
            $(".innerCollectionContainer", currentCollection).append(currentGridItemHtml);
        }); // each
        // once the template pages are installed, attach click handler to them.
        $(".invisibleThumbCover", currentCollection).each(function (index, div) {
            $(div).dblclick(function () {
                _this.addPageClickHandler();
            }); // invisibleThumbCover double click
            $(div).click(function (evt) {
                _this.thumbnailClickHandler(div, evt);
            }); // invisibleThumbCover click
        }); // each
        return indexToSelect;
    }; // LoadPagesFromCollection
    /**
     * Fires an event for C# to handle
     * @param {String} eventName
     * @param {String} eventData
     */
    PageChooser.prototype.fireCSharpEvent = function (eventName, eventData) {
        //console.log('firing CSharp event: ' + eventName);
        var event = new MessageEvent(eventName, { 'view': window, 'bubbles': true, 'cancelable': true, 'data': eventData });
        document.dispatchEvent(event);
    };
    return PageChooser;
})();
//# sourceMappingURL=page-chooser.js.map