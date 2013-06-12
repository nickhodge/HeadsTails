$(function () {

    var client = new WindowsAzure.MobileServiceClient('https://headstails.azure-mobile.net/', 'UmxHTxsGVDlkQUklKFQhxbOHvVWqRI32'),
        leaderboardTable = client.getTable('leaderboard');

    var headsTailsViewModel = {
        highestInARow: ko.observable('0'),
        currentInARow: ko.observable('0'),
        groupName: ko.observable('default'),
        currentUserId: ko.observable(),
        userName: ko.observable(),
        leaderBoard: ko.observableArray(),
        newusername: ko.observable()
    };

    function refreshLeaderboard() {
        var query = leaderboardTable.orderByDescending('highscore');
        headsTailsViewModel.leaderBoard.removeAll();
        query.read().then(function (leaderboard) {
            $.map(leaderboard, function (item) {
                headsTailsViewModel.leaderBoard.push(item);
            });
        });
    }

    function updateLeaderboard() {
        leaderboardTable.update({
            id: 1,
            highscore: headsTailsViewModel.highestInARow(),
            groupname: headsTailsViewModel.groupName()
        }).then(refreshLeaderboard);
    }

    // Game Mechanics, courtesy George Boole and Claude Shannon
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
        var isLoggedIn = client.currentUser !== null;
        $("#logged-in").toggle(isLoggedIn);
        $("#logged-out").toggle(!isLoggedIn);
        if (isLoggedIn) {
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
        refreshAuthDisplay();
        $('#summary').html('<strong>You must login to access data.</strong>');
    }
    
	document.addEventListener("deviceready", onDeviceReady, false);

	function onDeviceReady() {
	    ko.applyBindings(headsTailsViewModel);
        refreshAuthDisplay();
        $('#summary').html('<strong>You must login to access data.</strong>');
        $("#logged-out button").click(logIn);
        $("#logged-in button").click(logOut);
        refreshLeaderboard();
    }

});