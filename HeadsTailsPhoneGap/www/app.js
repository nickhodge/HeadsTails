$(function () {
    var client = new WindowsAzure.MobileServiceClient('https://headstails.azure-mobile.net/', 'UmxHTxsGVDlkQUklKFQhxbOHvVWqRI32'),
        leaderboardTable = client.getTable('leaderboard');

    var headsTailsViewModel = {
        highestInARow: ko.observable(0),
        currentInARow: ko.observable(0),
        groupName: ko.observable('default'),
        currentUserId: ko.observable(),
        userName: ko.observable(),
        leaderBoard: ko.observableArray(),
        newusername: ko.observable(),
        isLoggedIn: ko.observable(false),
        isLoggedOut: ko.observable(true),
        isLoadingData: ko.observable(false)
    };

    function refreshLeaderboard() {
        headsTailsViewModel.isLoadingData(true);
        var query = leaderboardTable.orderByDescending('highscore');
        headsTailsViewModel.leaderBoard.removeAll();
        query.read().then(function (leaderboard) {
            $.map(leaderboard, function (item) {
                headsTailsViewModel.leaderBoard.push(item);
            });
        });
        headsTailsViewModel.isLoadingData(false);
    }

    function updateLeaderboard() {
        leaderboardTable.update({
            id: headsTailsViewModel.currentUserId(),
            highscore: headsTailsViewModel.highestInARow(),
            groupname: headsTailsViewModel.groupName()
        }).then(refreshLeaderboard);
    }

    // Game Mechanics, courtesy George Boole and Claude Shannon
    // and if you do this an infinite number of times, Cantor style
    // 
    $('#choosetails').submit(function (evt) {
        chooseheads(false);
    });

    $('#chooseheads').click(function (evt) {
        chooseheads(true);
    });

    function chooseheads(flip) {
        var x = Math.random();
        if ((x < 0.5) && flip) {
            headsTailsViewModel.currentInARow(headsTailsViewModel.currentInARow() + 1);
        } else {
            headsTailsViewModel.currentInARow(0);
        }
        if (headsTailsViewModel.currentInARow() > headsTailsViewModel.highestInARow()) {
            //doupdate
            headsTailsViewModel.highestInARow(headsTailsViewModel.currentInARow());
            updateLeaderboard();
        }
    }


    // Handle refresh leaderboard
    $('#leaderboard-refresh').click(function (evt) {
        refreshLeaderboard();
    });

    // Handle Update username
    function updateUserName() {
        var newUserName = headsTailsViewModel.newusername();
        headsTailsViewModel.userName(newUserName);
        leaderboardTable.update({
            id: headsTailsViewModel.currentUserId(),
            highscore: headsTailsViewModel.highestInARow(),
            groupname: headsTailsViewModel.groupName(),
            username: headsTailsViewModel.userName()
        }).then(function (updateduser) {
            $("#form-username").toggle(false);
            refreshLeaderboard();
        });
    }

    // Handle Login/auth
    function refreshAuthDisplay() {
        //headsTailsViewModel.isLoggedIn = client.currentUser !== null;
        if (headsTailsViewModel.isLoggedIn() === false) {
            leaderboardTable.insert({
                groupname: headsTailsViewModel.groupName()
            }).then(function (newuser) {
                currentUserId = newuser.id;
                headsTailsViewModel.highestInARow(newuser.highscore);
                headsTailsViewModel.groupName(newuser.groupname);
                if (newuser.username !== "") {
                    headsTailsViewModel.userName(newuser.username);
                    $("#form-username").toggle(false);

                } else {
                    $("#form-username").toggle(true);
                    $("#saveusername").click(function (evt) {
                        updateUserName();
                    });
                }
                headsTailsViewModel.isLoggedIn(true);
                headsTailsViewModel.isLoggedOut(false);
                refreshLeaderboard();
            });
        }
    }

    function logIn() {
        client.login("microsoftaccount").then(refreshAuthDisplay, function (error) {
            alert(error);
        });
    }

    function logOut() {
        client.logout();
        headsTailsViewModel.isLoggedIn(false);
        headsTailsViewModel.isLoggedOut(true);
        refreshAuthDisplay();
    }

	document.addEventListener("deviceready", onDeviceReady, false);

	function onDeviceReady() {
	    ko.applyBindings(headsTailsViewModel);
         $("#logged-out button").click(logIn);
        $("#logged-in button").click(logOut);
        refreshLeaderboard();
    }

});