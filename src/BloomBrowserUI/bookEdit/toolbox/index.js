//review: does this actually do anything? After all, the whole point is that this isn't global
import * as $ from 'jquery';
import * as jQuery from 'jquery';

//doesn't help: (uses webpack script-loader) require('script!../../node_modules/jquery/dist/jquery.js');
//require('script!../../node_modules/jquery/dist/jquery.js');
import '../../modified_libraries/jquery-ui/jquery-ui-1.10.3.custom.min.js';
import 'localizationManager/localizationManager.js';
import 'jquery.i18n.custom.js';
import 'errorHandler.js';

import '../js/editableDivUtils.js';
import '../js/getIframeChannel.js';
import './decodableReader/directoryWatcher.js';
import './decodableReader/libsynphony/bloom_xregexp_categories.js';
import './decodableReader/libsynphony/jquery.text-markup.js';
import './decodableReader/libsynphony/synphony_lib.js';
import './decodableReader/libsynphony/bloom_lib.js';
import './decodableReader/synphonyApi.js';
import './decodableReader/jquery.div-columns.js';
import './decodableReader/readerSettings.js';
import './decodableReader/readerToolsModel.js';
import './decodableReader/readerTools.js';
import './decodableReader/decodableReader.js';
import './leveledReader/leveledReader.js';
import './talkingBook/talkingBook.js';
import './talkingBook/audioRecording.js';
import './bookSettings/bookSettings.js';
import './toolbox.js';