/**
 * The client side control of the people finder. Written by Ed&Josh.
 */

var IS_DEMO = false;
var SERVER = IS_DEMO ?
    'http://localhost:34781/api/values/'
    : 'http://app0111laka01.iaf/PplFndr/api/values/';

angular.module('phoneLocator', ['editableFieldDirective',
    'personTagsDirective', 'topBarDirective', 'bottomBarDirective',
    'mailingListDirective', 'personDirective', 'showPeopleDirective',
    'mainInputDirective', 'helpTextDirective']);

angular.module('phoneLocator').controller('FinderCtrl', function($scope, phoneService, httpExtension, errorReporter, logger) {
    var MINIMUM_QUERY_LENGTH = 2;

    window.phoneLocatorScope = $scope;
    window.phoneLocatorScope.peopleToEmail = [];

    

    $scope.peopleToEmail = [];
    
    $scope.AppStates = {
        DEFAULT: 0,
        SHOW_PEOPLE: 1,
        NOT_ENOUGH_CHARACTERS: 2,
        LOADING: 3,
        ERROR: 4
    };
    $scope.appState = $scope.AppStates.DEFAULT;

     // We would like to send a query every time the text changes but as this creates significant
     // overhead, we throttle them by only sending queries for ~1/8 text changes.
     // Additionally, if the user has stopped typing, we also want to send a query as throttling
     // is less of a concern if there is some gap between keystrokes.
     // To accomplish the latter, we set this timeout variable after every text change and cancel
     // the previous ones so that there is only one active timeout function tracking text updates
     // at a time.
     var sendQueryTimeout;
     $scope.onTextChange = function() {
        // We throttle by using a random number generator. Note that if the query is too short,
        // we handle it right away because we already know what to do with it.
        if (Math.random() * 8 >= 7 || isQueryTooShort()) {
            maybeDoQuery();
        } else {
            clearSendQueryTimeout();
            sendQueryTimeout = setTimeout(function() {
                maybeDoQuery();
            }, 500);
        }
     };

     // When submitting queries is manually triggered.
     $scope.submitQuery = function(opt_showAll, opt_forceMe) {
         maybeDoQuery(opt_showAll, opt_forceMe);
     };

     // A helper function to actually make the query.
     function maybeDoQuery(opt_showAll, opt_forceMe) {
        var queryTooShort = isQueryTooShort();
        
        if (queryTooShort) {
            currentRequestQuery = '';
            $scope.appState = $scope.query.length ?
                $scope.AppStates.NOT_ENOUGH_CHARACTERS : $scope.AppStates.DEFAULT;
            return;
        }

        var query = $scope.query;

        // Don't make the same request twice.
        if (query == currentRequestQuery && !opt_showAll) {
            return;
        }

        if (opt_showAll) {
            logger.logRequest(logger.LOG_TYPES.SEE_ALL_PRESSED);
        }
        
        // We also cancel the previous timeout every time we make a query as an added safety
        // mechanism (e.g., if the user presses enter to send a request).
        clearSendQueryTimeout();
        currentRequestQuery = $scope.query;

        $scope.appState = $scope.AppStates.LOADING;
        phoneService.get($scope.query, opt_showAll, opt_forceMe)
            .success(function(data) {
                handleResponse(data, query);
            })
            .error(function() {
                enterErrorState();
            });
        logger.logRequest(logger.LOG_TYPES.REQUEST, $scope.query);
     };

     // Tracks the query that was most recently sent. This makes sure that if a second query was sent
     // before the first one returns, we abandon the first response and stick with the second.
     var currentRequestQuery;
     function handleResponse(data, query) {
        if (query != currentRequestQuery) {
            // Abandon the response because we'll have a more updated one soon.
            return;
        }

        if (!data.length) {
            enterErrorState();
            return;
        }

        $scope.appState = $scope.AppStates.SHOW_PEOPLE;
        // The first item in data should be the metadata object. Everything else is people.
        $scope.metadata = data.splice(0,1)[0];
        $scope.people = data;
     };

     // Cancels the current query timeout (e.g., if we are making a new one).
     function clearSendQueryTimeout() {
        clearTimeout(sendQueryTimeout);
     };

     // A valid query must contain at least one space-separated word that is at least
     // MINIMUM_QUERY_LENGTH characters long.
     function isQueryTooShort() {
        var words = $scope.query.split(' ');
        for (var i = 0; i < words.length; i++) {
            if (words[i].length >= MINIMUM_QUERY_LENGTH) {
                return false;
            }
        }
        return true;        
     };

     // The main top bar contains the various Bama logos, etc. We only show when there is no
     // actual data to show.
     $scope.shouldHideMainTopBar = function() {
        return $scope.appState == $scope.AppStates.LOADING
        || ($scope.appState == $scope.AppStates.SHOW_PEOPLE && $scope.people.length);
     };

     $scope.searchFor = function(query, opt_showAll, opt_forceMe) {
        $scope.query = query;
        $scope.submitQuery(opt_showAll, opt_forceMe);
     };

     $scope.clearQuery = function() {
        $scope.searchFor('');
        document.getElementsByClassName('main-input')[0].focus()
     }

     // If the user taps on an email address field, close the extension and redirect to an email URL.
     $scope.sendEmail = function(emailAddress, opt_subject, opt_body) {
        logger.logRequest(logger.LOG_TYPES.MAIL);
        var emailUrl = 'mailto:' + emailAddress;
        if (opt_subject || opt_body) {
            emailUrl += '?subject=' + opt_subject + '&body=' + opt_body;
        } 

        window.open(emailUrl).close();
     };

     $scope.sendMailToPeople = function(persons) {
        var emails = '';
        for (var i = 0; i < persons.length; i++) {
            emails += persons[i].mail+';';
        }
        $scope.sendEmail(emails);
        $scope.clearPinned();
     };

     $scope.showSendMailToAllButton = function() {
        return $scope.people.length > 1
            && $scope.people.length < MAX_NUMBER_OF_EMAILS_IN_URL;
     }

     $scope.GetMailingListClass = function() {
        if($scope.shouldHideMainTopBar()) {
            return "mailing-list";
        }
        else {
            return "mailing-list-starter-page";
        }
    }


     // A person is "pinned" if they are in the email list. This toggles their pinned state.
     $scope.togglePinPerson = function(person) {
        if ($scope.isPinned(person)) {
            $scope.unpin(person);
        } else {
            pinPerson(person);
        }
     };

     $scope.clearPinned = function() {
        $scope.peopleToEmail = [];
        updatePinnedPersons();
     };

     function pinPerson(person) {
        if ($scope.peopleToEmail.length > MAX_NUMBER_OF_EMAILS_IN_URL) {
            alert('יותר מדי :(');
            return;
        }
        $scope.peopleToEmail.push(person);
        updatePinnedPersons();
     };

     $scope.isPinned = function(person) {
        for (var i = 0; i < $scope.peopleToEmail.length; i++) {
            if ($scope.peopleToEmail[i].mail == person.mail) {
                $scope.isPinnedText = "מחק מרשימת תפוצה";
                return true;
            }
        }
        $scope.isPinnedText = "הוסף לרשימת תפוצה";
        return false;
     };

     $scope.unpin = function(person) {
        var emailList = $scope.peopleToEmail.map(function(p){
            return p.mail;
        });
        var indexOfPerson = emailList.indexOf(person.mail);
        $scope.peopleToEmail.splice(indexOfPerson, 1);
        updatePinnedPersons();
     };

     function updatePinnedPersons() {
     }

     // Lots of functions that dictate when we show the more/less button for a person.
     $scope.showMoreButton = function(person) {
        return $scope.showMoreLessButton(person) && !$scope.showPersonExpanded(person);
    };

     $scope.showLessButton = function(person) {
        return $scope.showMoreLessButton(person) && $scope.showPersonExpanded(person);
     };

     $scope.showMoreLessButton = function(person) {
        return person.bottom_section.rows.length && ! $scope.forcePersonExpanded(person);
     };

     $scope.showPersonExpanded = function(person) {
        return person.showMore || $scope.forcePersonExpanded(person);
     };

     $scope.forcePersonExpanded = function(person) {
        return $scope.people.length == 1;
     };

     $scope.onMorePressed = function(person) {
        person.showMore = !person.showMore;
        if(person.showMore) {
            $scope.showMoreClass = "person-show-more";
        }
        else {
            $scope.showMoreClass = "";
        }
        logger.logRequest(logger.LOG_TYPES.MORE_PRESSED);
     };

     $scope.searchForBirthdays = function() {
        var query = 'ימי הולדת';
        if ($scope.me_as_person && $scope.me_as_person.department) {
            query += ' ' + $scope.me_as_person.department;
        }
        $scope.searchFor(query, true /* opt_showAll */);
     };

    // Initialization work.
    var allTags;
    httpExtension.sendGet(createRequestObject('initialmetadata')).success(function(data) {
        if (!data.length) {
            return;
        }
        var metadata = data[0];
        $scope.isAdmin = metadata.is_admin;

        $scope.allTagsGrouped = metadata.tags_to_add_grouped;

        // TODO(Josh): Make this not a list.
        if (metadata.me_as_person.length) {
            setMeAsPerson(metadata.me_as_person[0]);
        }

        $scope.nonAdminsCanAddTags = !!metadata.non_admins_can_add_tags;
    });


    //chrome.storage.sync.get(['meAsPerson'], function(persons) {
    //    if(persons['meAsPerson']) {
    //        setMeAsPerson(persons['meAsPerson']);
    //    }
    //});

    logger.logRequest(logger.LOG_TYPES.INITIAL);

    errorReporter.onInitialization();
    // End initialization work.

    function setMeAsPerson(meAsPerson) {
        window.phoneLocatorScope.me_as_person = meAsPerson;
    };

    function enterErrorState() {
        $scope.appState = $scope.AppStates.ERROR;
        errorReporter.noteError();
    };
});

angular.module('editableFieldDirective', [])
    .controller('EditableFieldController', ['$scope', function($scope) {
         $scope.canEditField = function(person, field) {
            return $scope.canEditPersonalText(person) && field.editApiName;
         };

         $scope.maybeStartEditingField = function(person, field) {
            if (!$scope.canEditField(person, field)) {
                return;
            }
            field.isBeingEdited = true;
            field.valueToAdd = field.value;
         };

        $scope.setPersonalTextField = function(person, field) {
            if (!field.editApiName) {
                return;
            }
            // TODO(Josh): Because there is data valdiation that happens on the server
            // (e.g., in the case of a phone number), we might want to show an error
            // message here. Or maybe we should just let it be because it's the user's
            // fault for putting up an unhelpful phone number.
            $scope.setPersonalTextHelper(
                person,
                field.editApiName,
                field.valueToAdd,
                function(value) { field.value = value; },
                function(value) { field.isBeingEdited = false; });
         };

         $scope.isNameField = function(field) {

            return field.name == "שם";

         }
    }])
    .directive('editableField', function() {
        return {
            templateUrl: 'editable-field.html'
        };
    });

angular.module('personTagsDirective', [])
    .controller('PersonTagsController', ['$scope', 'httpExtension', function($scope, httpExtension) {
         $scope.getTagColor = function(type) {
            switch (type) {
                case 'שימוש':
                    return 'rgb(240,88,90)';
                case 'שפות':
                    return 'rgb(219,208,24)';
                case 'תחביבים':
                    return 'rgb(68,179,204)';
                case 'אקדמיה':
                    return 'rgb(253,149,255)';
                case 'יכולות':
                    return 'rgb(55, 170, 48)';
                case 'שונות':
                    return 'rgb(240, 140, 50)';
                case 'קהילות':
                    return 'rgb(228, 135, 213)';
                case 'השכלה':
                    return 'rgb(0,255,0)';
                default:
                    return 'rgb(166,166,166)';
            }
         }
         ;

         $scope.searchForTag = function(tag) {
            $scope.searchFor('#' + tag);
         };

         $scope.showTagsToAdd = function() {
            $scope.setTagsToAdd();
            $scope.person.show_tag_adding = true;
         }

         // Resets the list of tags which the current user can add for the current person.
         $scope.setTagsToAdd = function() {
            // See if we are already highlighting one of tha tag types.
            var currentlyShownTagType =
                $scope.person.tagTypes && $scope.person.tagTypes.find(function(tagType) {
                    return tagType.isBeingShown;
                });
            var currentlyShownTagTypeTitle = 
                currentlyShownTagType && currentlyShownTagType.title;

            // Clear the list.
            $scope.person.tagTypes = [];
             // Tags come in a group, which is a collection of a title (e.g., 'Languages')
             // and a list of tags.
            $scope.allTagsGrouped.forEach(function(tagGroup) {
                var tagGroupForPerson = {};
                $scope.person.tagTypes.push(tagGroupForPerson);

                tagGroupForPerson.title = tagGroup.type;
                tagGroupForPerson.tags = [];
                tagGroupForPerson.isBeingShown =
                    currentlyShownTagTypeTitle === tagGroupForPerson.title;
                tagGroup.tags.forEach(function(tagWrapper){
                    if (!personHasTag(tagWrapper.tag)) {
                        tagGroupForPerson.tags.push(tagWrapper.tag);
                    }
                });
            });
         };

         $scope.showTagTypeToAdd = function(tagTypeToShow) {
            var wasTagTypeBeingShown = tagTypeToShow.isBeingShown;
            $scope.person.tagTypes.forEach(function(tagType) {
                tagType.isBeingShown = false;
            });
            tagTypeToShow.isBeingShown = !wasTagTypeBeingShown;
         };

         function personHasTag(tag) {
            return !!$scope.person.tags.find(function(personTag) {
                return personTag.tag == tag;
            });
         };

         $scope.addTag = function(newTag) {
            var requestObject = createRequestObject('addtag', newTag);
            requestObject.MisparIshi = $scope.person.mispar_ishi;
            httpExtension.sendGet(requestObject).success(function(data) {
                if (!data.length || !data[0].new_tag) {
                    alert('הגעת למספר התגיות המקסימלי אותו אנחנו מאפשרים במועד זה. אנחנו עובדים על הגדלת הכמות! \nעד אז, על מנת להוסיף תגית חדשה, תאלץ למחוק תגית ישנה.');
                    return;
                }
                $scope.person.tags.push(data[0].new_tag);
                $scope.setTagsToAdd();
            });
         };

         $scope.deleteTag = function(tagToDelete) {
            var postObject = createRequestObject('deletetag', tagToDelete.tag);
            postObject.MisparIshi = $scope.person.mispar_ishi;
            httpExtension.sendPost(postObject).success(function(data) {
                var indexOfTagToDelete = $scope.person.tags.indexOf(tagToDelete);
                if (indexOfTagToDelete != -1) {
                    $scope.person.tags.splice(indexOfTagToDelete, 1);
                }
                $scope.setTagsToAdd();
            });
         };

         $scope.canAddTags = function() {
            return ($scope.person.is_me && $scope.nonAdminsCanAddTags) || $scope.isAdmin;
         };

         $scope.canDeleteTag = function(tag) {
            return ($scope.person.is_me && tag.non_admins_can_edit) || $scope.isAdmin;
         };
    }])
    .directive('personTags', function() {
        return {
            templateUrl: 'person-tags.html'
        };
    });

angular.module('topBarDirective', [])
    .controller('TopBarController', ['$scope', function($scope) {
        $scope.getGreeting = function() {
            if (!$scope.me_as_person) {
                return;
            }
            var currentHour = new Date().getHours();
            var timeOfDayPart = currentHour < 6 ? 'לילה טוב'
                : currentHour < 12 ? 'בוקר טוב'
                : currentHour < 18 ? 'צהריים טובים'
                : currentHour < 22 ? 'ערב טוב'
                : 'לילה טוב';
            var name = $scope.me_as_person.name;
            return timeOfDayPart + ', ' + name + '!';
        }

        $scope.searchForMe = function() {
            if (!window.phoneLocatorScope.me_as_person) {
                return;
            }
            var fullNameToUse = window.phoneLocatorScope.me_as_person.full_name || 'אני';
            $scope.searchFor(
                fullNameToUse,
                true /* opt_showAll */,
                true /* opt_forceMe */);
        };

         $scope.goToBama = function() {
            //chrome.tabs.create({ url: BAMA_LINK});
            window.open(BAMA_LINK);
         };
    }])
    .directive('topBar', function() {
        return {
            templateUrl: 'top-bar.html'
        };
    });

angular.module('bottomBarDirective', [])
    .controller('BottomBarController', ['$scope', 'httpExtension', 'logger', function($scope, httpExtension, logger) {
         $scope.BugReporterStates = {
            INITIAL: '1',
            REPORTING: '2',
            LOADING: '3',
            REPORTED: '4'
         };
         
         $scope.bugReportState = $scope.BugReporterStates.INITIAL;
         $scope.reportBug = function() {
            var requestObject = createRequestObject('reportoddity', $scope.bugToReport);
            httpExtension.sendPost(requestObject).success(function() {
                $scope.bugReportState =  $scope.BugReporterStates.REPORTED;
            });
            $scope.bugReportState =  $scope.BugReporterStates.LOADING;
         };

         $scope.share = function() {
            logger.logRequest(logger.LOG_TYPES.SHARED);
            var emailText = 'היי!\n\n'
                + 'רציתי לשתף אתכם במערכת הבמה לאנשים - מערכת חיפוש חופשי לאנשי החיל.\n\n'
                + 'מדריך התקנה: ' + INSTALLATION_INSTRUCTIONS_LINK;
            $scope.sendEmail(
                '' /* emailAddress */,
                'הבמה לאנשים',
                encodeURIComponent(emailText));
        };
    }])
    .directive('bottomBar', function() {
        return {
            templateUrl: 'bottom-bar.html'
        };
    });

angular.module('mailingListDirective', [])
    .directive('mailingList', function() {
        return {
            templateUrl: 'mailing-list.html'
        };
    });

angular.module('helpTextDirective', [])
    .directive('helpText', function() {
        return {
            templateUrl: 'help-text-floater.html'
        };
    });

angular.module('personDirective', [])
    .controller('PersonController', ['$scope', 'httpExtension', function($scope, httpExtension) {
        // For when we want to update a person's updatable field (such as phone number or sex). 
        $scope.setPersonalTextHelper = function(person, apiName, valueToSend, setFieldFunction, stopEditingFunction) {
            var postObject = createRequestObject(apiName, valueToSend);
            postObject.MisparIshi = person.mispar_ishi;
            httpExtension.sendPost(postObject).then(function() {
                setFieldFunction(valueToSend);
            });
            stopEditingFunction();
            // TODO(Josh): Is this a hack? It makes the field say something until we get the value.
            setFieldFunction('נטען...');
        };

        $scope.canEditPersonalText = function(person) {
            return person.is_me || $scope.isAdmin;
         };

         $scope.sendBirthday = function() {
            if (!$scope.person.mail) {
                return;
            }
            $scope.sendEmail($scope.person.mail, 'מזל טוב!', 'יום הולדת שמח!!!')
         };
         $scope.CheckIfSpecialField = function(fields){
            return fields.filter(function(field) {
                return !(field.name == "שם" || field.name == "תפקיד");
            });
        };
    }])
    .directive('person', function() {
        return {
            templateUrl: 'person.html'
        };
    });

angular.module('showPeopleDirective', [])
    .directive('showPeople', function() {
        return {
            templateUrl: 'show-people.html'
        };
    });

angular.module('mainInputDirective', [])
    .directive('mainInput', function() {
        return {
            templateUrl: 'main-input.html'
        };
    });

angular.module('phoneLocator').service('phoneService', function(httpExtension) {
    return {
        // Already assumes that data validation has been done on query (e.g., for length).
        get: function(query, opt_showAll, opt_forceMe) {
            var request = createRequestObject('query', query);
            request.ShowAll =  !!opt_showAll;
            request.ForceMe =  !!opt_forceMe;
            return httpExtension.sendGet(request);
        }
    };
});

// If we ever have an error, (i.e., entered the "please contact Adam" error state),
// we want to report it to the server so that it apepars on the dashboard.
// Of course, there is a good chance that the report will fail too if the server is
// down. So the way around this is to try sending the error and if it fails to store it
// in memory instead. Then every time we initialize, we see if there is an error stored
// in memory and if so we try sending it again.
angular.module('phoneLocator').service('errorReporter', function(httpExtension) {
    // Try sending the given error to the server.
    // If the post fails, store it in memory instead.
    function sendError(newError) {   
        var requestObject = createRequestObject('reportoddity', newError);
        httpExtension.sendPost(requestObject).error(function() {
        });
    };

    return {
        onInitialization: function() {
        },
        
        noteError: function() {
            var newError = 'On '
                + new Date().toTimeString()
                + ', an error occured for user '
                + window.phoneLocatorScope.me_as_person.full_name
                + ' - '
                + window.phoneLocatorScope.me_as_person.mispar_ishi;
            sendError(newError);
        },
    }
});

angular.module('phoneLocator').service('logger', function(httpExtension) {
    // It's assumed this is a reasonable guarantee of uniqueness.
    var sessionId = Date.now() + Math.random();
    var logTypes =  {
        INITIAL: 'initial',
        MAIL: 'mail',
        REQUEST: 'request',
        MORE_PRESSED: 'morepressed',
        SEE_ALL_PRESSED: 'seeallpressed',
        SHARED: 'shared'
    };
    return {
        logRequest: function(logType, opt_query) {
            var postObject = createRequestObject('log');
            postObject.Logs = {
                SessionId: sessionId,
                LogType: logType
            };
            var logString = sessionId + ',' + logType;
            if (logType == logTypes.REQUEST) {
                postObject.Logs.Query = opt_query;
            }

            httpExtension.sendPost(postObject);
        },
        LOG_TYPES: logTypes,
    };
});

angular.module('phoneLocator').service('httpExtension', function($http) {
    return {
        sendPost: function(requestObject) {
            // TODO(josh): Do we need this '1'?
            return $http.post(SERVER + '1', requestObject);
        },

        sendGet: function(requestObject) {
            return $http({
                url: SERVER,
                method: 'GET',
                params: requestObject
            });
        }
    };
});

function createRequestObject(type, opt_data) {
    return {
        Type: type,
        Data: opt_data,
    };
};

var MAX_NUMBER_OF_EMAILS_IN_URL = 100;
var BAMA_LINK = 'http://web-0108a.iaf/Innovation/Items?cat=4';
var INSTALLATION_INSTRUCTIONS_LINK = 'http://app0111laka01.iaf/PplFndr';