/**
 * jquery.text-markup.js
 *
 * Marking text according to various rules
 *
 * Created Apr 24, 2014 by Phil Hopper
 *
 */

import * as _ from "underscore";
import { theOneLibSynphony, LibSynphony } from "./synphony_lib";
import "./bloomSynphonyExtensions.js"; //add several functions to LanguageData
import { ReaderToolsModel } from "../readerToolsModel";

/**
 * Use an 'Immediately Invoked Function Expression' to make this compatible with jQuery.noConflict().
 * @param {jQuery} $
 */
($ => {
    var cssSentenceTooLong = "sentence-too-long";
    var cssSightWord = "sight-word";
    var cssWordNotFound = "word-not-found";
    var cssPossibleWord = "possible-word";
    var cssDesiredGrapheme = "desired-grapheme";
    var cssTooMuchStuffOnPage = "page-too-many-words-or-sentences";
    const cssWordTooLong = "word-too-long";

    /**
     * Checks the innerHTML of an HTML entity (div) using the selected options
     * @param {Object} options
     * @returns {Object}
     */
    $.fn.checkLeveledReader = function(options) {
        var allWords = "";
        const longWords: string[] = [];

        const opts = $.extend(
            {
                maxWordsPerSentence: Infinity,
                maxWordsPerPage: Infinity,
                maxSentencesPerPage: Infinity,
                maxGlyphsPerWord: Infinity
            },
            options
        );

        // 0 means unlimited. So convert them to Infinity
        if (opts.maxWordsPerSentence <= 0) {
            opts.maxWordsPerSentence = Infinity;
        }
        if (opts.maxWordsPerPage <= 0) {
            opts.maxWordsPerPage = Infinity;
        }
        if (opts.maxSentencesPerPage <= 0) {
            opts.maxSentencesPerPage = Infinity;
        }

        // remove previous synphony markup
        this.removeSynphonyMarkup();

        // initialize words per page
        var totalWordCount = 0;
        // initialize sentences per page
        let totalSentenceCount = 0;

        var checkLeaf = leaf => {
            stashNonTextUIElementsInEditBox(leaf);
            // split into sentences. We need it both with markup
            // (to preserve bold/italic/ckEditor landmarks in the output)
            // and without (because some markup, especially ckEditor invisible landmarks,
            // may alter word counts and lists)
            var fragments = theOneLibSynphony.stringToSentences($(leaf).html());

            var newHtml = "";

            for (var i = 0; i < fragments.length; i++) {
                var fragment = fragments[i];

                if (fragment.isSpace) {
                    // this is inter-sentence space
                    newHtml += fragment.text;
                    allWords += " ";
                } else {
                    // This is basically duplicating how stringToSentences comes up with
                    // fragment.text but with removeAllHtmlMarkupFromString applied.
                    // I don't much like that duplication. But we need removeAllHtmlMarkupFromString
                    // so that the words we count won't be messed up by (e.g.) invisible spaces
                    // that ckEdit puts in as bookmarks to keep our place. We can't apply it
                    // to the input to stringToSentences, because we want to preserve the markup
                    // and bookmark when we put the fragments back together to make the new
                    // text. I tried making two parallel fragments arrays, one using the
                    // unmodified text, and one from removeAllHtmlMarkupFromString; but in general they
                    // don't come out the same length, or with pieces corresponding. For example,
                    // removeAllHtmlMarkupFromString cleans out <br>, which otherwise becomes an element in
                    // the list.
                    const cleanText = removeAllHtmlMarkupFromString(
                        fragment.text
                    );
                    const words = theOneLibSynphony.getWordsFromHtmlString(
                        cleanText
                    );
                    if (opts.maxGlyphsPerWord > 0) {
                        for (const w of words) {
                            if (
                                ReaderToolsModel.getWordLength(w) >
                                opts.maxGlyphsPerWord
                            ) {
                                longWords.push(w);
                            }
                        }
                    }
                    var sentenceWordCount = words.length;
                    totalWordCount += sentenceWordCount;
                    allWords += cleanText;
                    if (sentenceWordCount) ++totalSentenceCount;

                    // check sentence length
                    if (sentenceWordCount > opts.maxWordsPerSentence) {
                        // tag sentence as having too many words
                        newHtml +=
                            '<span class="' +
                            cssSentenceTooLong +
                            '" data-segment="sentence">' +
                            fragment.text +
                            "</span>";
                    } else {
                        // nothing to see here
                        newHtml += fragment.text;
                    }
                }
            }

            // If this element represents a paragraph, then the overall page text needs a paragraph break here.
            if (leaf.tagName === "P") {
                allWords += "\r\n";
            }
            if (longWords.length) {
                newHtml = theOneLibSynphony.wrap_words_extra(
                    newHtml,
                    longWords,
                    cssWordTooLong,
                    ' data-segment="word"'
                );
            }

            // set the html
            $(leaf).html(newHtml);
            restoreNonTextUIElementsInEditBox(leaf);
        };

        var checkRoot = root => {
            var children = root.children();
            var processedChild = false; // Did we find a significant child?
            for (var i = 0; i < children.length; i++) {
                var child = children[i];
                var name = child.nodeName.toLowerCase();
                // Review: is there a better way to pick out the elements that can occur within content elements?
                if (
                    name != "span" &&
                    name != "br" &&
                    name != "i" &&
                    name != "b" &&
                    name != "u" &&
                    name != "em" &&
                    name != "strong" &&
                    name != "sup"
                ) {
                    processedChild = true;
                    checkRoot($(child));
                }
            }
            if (!processedChild)
                // root is a leaf; process its actual content
                checkLeaf(root.get(0));
            // Review: is there a need to handle elements that contain both sentence text AND child elements with their own text?
        };

        this.each(function() {
            checkRoot($(this));
        });
        // highlight the page for too many words or sentences found
        // (or remove any previous highlighting if it's all okay now)
        let pageDiv: JQuery;
        const page = parent.window.document.getElementById(
            "page"
        ) as HTMLIFrameElement;
        if (!page || !page.contentWindow) {
            pageDiv = $("body").find("div.bloom-page");
        } else {
            pageDiv = $("body", page.contentWindow.document).find(
                "div.bloom-page"
            );
        }
        if (
            totalWordCount > opts.maxWordsPerPage ||
            totalSentenceCount > opts.maxSentencesPerPage
        ) {
            pageDiv.addClass(cssTooMuchStuffOnPage);
        } else {
            pageDiv.removeClass(cssTooMuchStuffOnPage);
        }

        this["allWords"] = allWords;
        return this;
    };

    /**
     * Checks the innerHTML of an HTML entity (div) using the selected options
     * @param {Object} options
     * @returns {Object}
     */
    $.fn.checkDecodableReader = function(options) {
        var opts = $.extend(
            {
                focusWords: [],
                previousWords: [],
                sightWords: [],
                knownGraphemes: []
            },
            options
        );
        var text = "";

        // remove previous synphony markup
        this.removeSynphonyMarkup();

        // get all text
        this.each(function() {
            text += " " + removeAllHtmlMarkupFromString($(this).html());
        });

        /**
         * @type StoryCheckResults
         */
        var results = theOneLibSynphony.checkStory(
            opts.focusWords,
            opts.previousWords,
            opts.knownGraphemes,
            text,
            opts.sightWords.join(" ")
        );

        // markup
        this.each(function() {
            stashNonTextUIElementsInEditBox(this);
            var html = $(this).html();

            // ignore empty elements
            if (html.trim().length > 0 && text.trim().length > 0) {
                html = theOneLibSynphony.wrap_words_extra(
                    html,
                    results.sight_words,
                    cssSightWord,
                    ' data-segment="word"'
                );
                html = theOneLibSynphony.wrap_words_extra(
                    html,
                    results.possible_words,
                    cssPossibleWord,
                    ' data-segment="word"'
                );

                // remove numbers from list of bad words
                var notFound = _.difference(
                    results.remaining_words,
                    results.getNumbers()
                );

                html = theOneLibSynphony.wrap_words_extra(
                    html,
                    notFound,
                    cssWordNotFound,
                    ' data-segment="word"'
                );
                $(this).html(html);
            }
            restoreNonTextUIElementsInEditBox(this);
        });

        return this;
    };

    /**
     * Finds the maximum word count in the selected sentences.
     * @returns {int}
     */
    $.fn.getMaxSentenceLength = function() {
        var maxWords = 0;

        this.each(function() {
            // split into sentences
            var fragments = theOneLibSynphony.stringToSentences(
                removeAllHtmlMarkupFromString($(this).html())
            );

            if (!fragments || fragments.length === 0) return;

            // remove inter-sentence space
            fragments = fragments.filter(frag => {
                return frag.isSentence;
            });

            var subMax = Math.max.apply(
                Math,
                fragments.map(frag => {
                    return frag.wordCount();
                })
            );

            if (subMax > maxWords) maxWords = subMax;
        });

        return maxWords;
    };

    /**
     * Returns the count of all words in the selected elements.
     * @returns {int}
     */
    $.fn.getTotalWordCount = function() {
        var wordCount = 0;

        this.each(function() {
            // split into sentences
            var fragments = theOneLibSynphony.stringToSentences(
                removeAllHtmlMarkupFromString($(this).html())
            );

            // remove inter-sentence space
            fragments = fragments.filter(frag => {
                return frag.isSentence;
            });

            // sum of word counts
            for (var i = 0; i < fragments.length; i++)
                wordCount += fragments[i].wordCount();
        });

        return wordCount;
    };

    /**
     * Removes all the markup that was inserted by this addin
     */
    $.fn.removeSynphonyMarkup = function() {
        this.each(function() {
            // remove markup for deleted text
            $(this)
                .find("span[data-segment=sentence]:empty")
                .remove();
            $(this)
                .find("span[data-segment=word]:empty")
                .remove();
            $(this)
                .find("span[data-segment=grapheme]:empty")
                .remove();

            // remove previous sentence markup
            $(this)
                .find("span[data-segment=sentence]")
                .contents()
                .unwrap();
            $(this)
                .find("span[data-segment=word]")
                .contents()
                .unwrap();
            $(this)
                .find("span[data-segment=grapheme]")
                .contents()
                .unwrap();
        });

        // remove page markup
        var page = parent.window.document.getElementById(
            "page"
        ) as HTMLIFrameElement;
        if (!page || !page.contentWindow)
            $("body")
                .find("div." + cssTooMuchStuffOnPage)
                .removeClass(cssTooMuchStuffOnPage);
        else
            $("body", page.contentWindow.document)
                .find("div." + cssTooMuchStuffOnPage)
                .removeClass(cssTooMuchStuffOnPage);
    };

    $.extend({
        /**
         * Highlights selected graphemes in a word
         * @param {String} word
         * @param {String[]} gpcForm
         * @param {String[]} desiredGPCs
         * @returns {String}
         */
        markupGraphemes: (word, gpcForm, desiredGPCs) => {
            // for backward compatibility
            if (Array.isArray(word)) return oldMarkup(word, gpcForm);

            var returnVal = "";

            // loop through GPCForm
            for (var i = 0; i < gpcForm.length; i++) {
                var offset = gpcForm[i].length;
                var chars = word.substr(0, offset);

                if (desiredGPCs.indexOf(gpcForm[i]) > -1)
                    returnVal +=
                        '<span class="' +
                        cssDesiredGrapheme +
                        '" data-segment="grapheme">' +
                        chars +
                        "</span>";
                else returnVal += chars;

                word = word.slice(offset);
            }

            return returnVal;
        }
    });

    $.extend({
        cssSentenceTooLong: () => {
            return cssSentenceTooLong;
        }
    });
    $.extend({
        cssSightWord: () => {
            return cssSightWord;
        }
    });
    $.extend({
        cssWordNotFound: () => {
            return cssWordNotFound;
        }
    });
    $.extend({
        cssPossibleWord: () => {
            return cssPossibleWord;
        }
    });

    function oldMarkup(gpcForm, desiredGPCs) {
        var returnVal = "";

        // loop through GPCForm
        for (var i = 0; i < gpcForm.length; i++) {
            if (desiredGPCs.indexOf(gpcForm[i]) > -1)
                returnVal +=
                    '<span class="' +
                    cssDesiredGrapheme +
                    '" data-segment="grapheme">' +
                    gpcForm[i] +
                    "</span>";
            else returnVal += gpcForm[i];
        }

        return returnVal;
    }

    /**
     * The formatButton is a div at the end of the editable text that needs to be ignored as we scan and markup the text box.
     * It should be restored witha a call to restoreFormatButton();
     **/

    var stashedFormatButton;

    function stashNonTextUIElementsInEditBox(element) {
        stashedFormatButton = $(element).find("#formatButton");
        if (stashedFormatButton) {
            stashedFormatButton.remove();
        }
    }
    /**
     * The formatButton is a div at the end of the editable text that needs to be ignored as we scan and markup the text box.
     * Calls to this should be preceded by a call to stashFormatButton();
     **/
    function restoreNonTextUIElementsInEditBox(element) {
        if (stashedFormatButton) {
            $(element).append(stashedFormatButton);
            stashedFormatButton = null;
        }
    }
})(jQuery);

/**
 * Strip the HTML markup from a string
 * @param {string} textHtml
 * @returns {string}
 */
export function removeAllHtmlMarkupFromString(textHtml: string): string {
    // ensure spaces after line breaks and paragraph breaks
    var regex = /(<br><\/br>|<br>|<br ?\/>|<p><\/p>|<\/?p>|<p ?\/>|\n)/g;
    textHtml = textHtml.replace(regex, " ");

    // This regex is rather specific to the spans ckeditor sticks in as
    // 'landmarks' so the selection can be restored after manipulating the
    // markup. In principle we could have a more complex regex that would
    // remove all display:none spans, even if there are other explicit styles
    // or with single quotes around the style or with different white space.
    // However, we don't have a current need for it, so the extra
    // complication doesn't seem worthwhile.
    var ckeRegex = /<span [^>]*style="display: none;"[^>]*>[^<]*<\/span>/g;
    textHtml = textHtml.replace(ckeRegex, "");

    // Both open and close tags for markup
    var markupRegex = /<\/?(strong|em|sup|u|i|b|a|span)>/g;
    textHtml = textHtml.replace(markupRegex, "");

    // Open tags for more complex markup (ie, span and a tags).
    var complexMarkupRegex = /<(span|a)[ \r\n\t][^>]*>/g;
    textHtml = textHtml.replace(complexMarkupRegex, "");

    return $("<div>" + textHtml + "</div>").text();
}
