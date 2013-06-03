$(function () {
    var client = new WindowsAzure.MobileServiceClient('https://headstails.azure-mobile.net/', 'UmxHTxsGVDlkQUklKFQhxbOHvVWqRI32'),
    leaderboardTable = client.getTable('leaderboard');

	var highestInARow = 0;	
	var currentInARow = 0;
	var groupName = "default";
	var currentUserId = 0;
	
   function refreshLeaderboard() {
        var query = leaderboardTable.orderByDescending('highscore');
		 
        query.read().then(function(leaderboard) {
            var leaderboarditems = $.map(leaderboard, function(item) {
                return $('<li>').html(item.username + '&nbsp;<em> Highscore: '+ item.highscore +'</em>');
            });

            $('#leaderboard-items').empty().append(leaderboarditems).toggle(leaderboarditems.length > 0);
            $('#summary').html('<strong>' + leaderboarditems.length + '</strong> members in group <strong>' + groupName + '</strong>');
        });
    }
	
	function updateLeaderboard () {
		leaderboardTable.update({id:1 , highscore: highestInARow, groupname: groupName}).then(refreshLeaderboard);
	}
	
	// Game Mechanics, courtesy George Boole and Claude Shannon
    $('#choosetails').submit(function(evt) {
        chooseheads(false);
    });
	
    $('#chooseheads').click(function(evt) {
        chooseheads(true);
    });
	
	function chooseheads(flip) {
		var x = Math.random();
		if ((x < 0.5) && flip) {
			currentInARow++;
		} else {
			currentInARow = 0;
		}
		$('#current-in-a-row').html(currentInARow);
		if (currentInARow > highestInARow) {
		//doupdate
			highestInARow = currentInARow;
			$('#highest-in-a-row').html(highestInARow);
			updateLeaderboard();
		}
	}

	
	// Handle refresh leaderboard
    $('#leaderboard-refresh').click(function(evt) {
        refreshLeaderboard();
    });
	
	// Handle Update username
	function updateUserName() {
		var newUserName = $('input[id=newusername]').val();
		leaderboardTable.update({id:currentUserId , highscore: highestInARow, groupname: groupName, username: newUserName }).then(function(updateduser)
		{
			$("#login-name").text(updateduser.username);
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
				groupname: groupName
			}).then(function (newuser){
				currentUserId = newuser.id;
				highestInARow = newuser.highscore;
				groupName = newuser.groupname;
				$('#highest-in-a-row').html(highestInARow);
				if (newuser.username !== "") {
					$("#login-name").text(newuser.username);
				   $("#form-username").toggle(false);
				   
				} else {
					$("#form-username").toggle(true);
					$("#saveusername").click(function(evt) {
						updateUserName();
					});
				}
				refreshLeaderboard();
			});
		}
	}

	function logIn() {
		client.login("microsoftaccount").then(refreshAuthDisplay, function(error){
		alert(error); });
	}

	function logOut() {
		client.logout();
		refreshAuthDisplay();
		$('#summary').html('<strong>You must login to access data.</strong>');
	}
    
	document.addEventListener("deviceready", onDeviceReady, false);

	function onDeviceReady() {
	    
        refreshAuthDisplay();
        $('#summary').html('<strong>You must login to access data.</strong>');
        $("#logged-out button").click(logIn);
        $("#logged-in button").click(logOut);
        refreshLeaderboard();
    }

});