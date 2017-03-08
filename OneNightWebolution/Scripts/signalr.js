var hub = $.connection.oneNightWebolutionHub;

$.connection.hub.start().done(function () {
    $('#join').click(function () {
        hub.server.addPlayerToParty($('#playerNameInput').val(), $('#partynameinput').val());

    });

    $('#startButton').click(function () {
        hub.server.BeginGame($('#gameid').val());
    });

});

hub.client.showPartyName = function (partyName) {
    $('#partyName').text(partyName);
};

hub.client.showPlayerName = function (playerName) {
    $('#playerName').text(playerName);
};

hub.client.showOtherPlayer = function (otherPlayerName, otherPlayerID) {
    $('#otherplayers').append('<div id=' + otherPlayerID +
        ' class="otherplayercontainer"><div class="role">Role: Unknown</div><div class="specialist">Specialist: Unknown</div><div>'
        + otherPlayerName + '</div>');
};

hub.client.showStartButton = function () {
    $('startButton').removeClass('hidden');
};

hub.client.setPlayerID = function (playerID) {
    $('#playerid').text(playerID);
};

hub.client.setPartyID = function (gameID) {
    $('#gameid').text(gameID);
};

hub.client.showID = function (isTraitor) {
    if (isTraitor) {
        $('#rolebox').text("Traitor");
    }
    else {
        $('#rolebox').text("Rebel");
    }
};
hub.client.showSpecialist = function (specialist) {
    $('#specialist').text(specialist);
};

hub.client.showSpecialist = function (specialist, playerID) {
    $('#playerID').find('.specialist').text(specialist);
};

hub.client.showGameBegun = function () {
    $('#gamestate').text("Game begun");
};

hub.client.showOtherRole = function (playerID, role) {
    $('#playerID').find('.role').text(role);
}

hub.client.takeTurn = function () {
    $('#gamestate').text("Your Turn");
    var singleClickSpecialists = ["investigator", "thief", "analyst", "confirmer", "revealer"];
    var multiClickSpecialists = ["reassinger", "signaller"];
    
    if (getPlayerSpecialist() in singleClickSpecialists) {
        $('.otherplayercontainer').on('click', function () {
            var selectedID = $(this).attr('id');

            hub.server.takeSingleAction(getPlayerID(), getPlayerSpecialist(), selectedID);
        });
    }
    else if (getPlayerSpecialist() == "signaller") {
        if (getplayerRole() == "rebel") {
            $('.otherplayercontainer').on('click', function () {
                var selectedID = $(this).attr('id');

                hub.server.takeSingleAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), selectedID);
            });
        }
        else {
            rolecontainers = $('.role')
            for (role in rolecontainers) {
                if ($(role).text() == "rebel") {
                    $(role).parent().on('click', function () {
                        var selectedID = $(this).attr('id');

                        hub.server.takeSingleAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), selectedID);
                    });
                }
            }
        }
    }
    else if (getPlayerSpecialist == "reassigner") {
        $('#takeactionbutton').removeClass('hidden');
        var swapIDArray = [];

        $('.otherplayercontainer').on('click', function () {
            swapIDArray.push($(this).attr('id'));
            this.setAttribute("class", "otherplayercontainer selectedplayer")
            if (getPlayerRole == "rebel") {
                if (swapIDArray.length == 2) {
                    hub.server.takeReassignAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), swapIDArray[0], swapIDArray[1]);
                }
            }
            else {
                hub.server.takeReassignAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), swapIDArray[0], swapIDArray[1])
            }
        });
    }
};

hub.client.showTapMessage = function (name) {
    $('#message').text('someone tapped you');
};

hub.client.showFullTraitorsMessage = function () {
    $('#message').text('Already full of traitors');
}

getPlayerID = function () {
    return $('#playerid').text();
};

getPlayerSpecialist = function (){
    return $('#specialist').text();
};

getPlayerRole = function () {
    return $('#rolebox').text();
};

getPartyName = function () {
    return $('#partyName').text();
};