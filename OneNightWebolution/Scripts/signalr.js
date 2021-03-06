﻿var hub = $.connection.oneNightWebolutionHub;

$.connection.hub.start().done(function () {
    $('#join').click(function () {
        $('#playerNameInput').addClass('hidden');
        $('#partynameinput').addClass('hidden');
        $('#join').addClass('hidden');
        hub.server.addPlayerToParty($('#playerNameInput').val(), $('#partynameinput').val());
        
    });

    $('#startButton').click(function () {
        hub.server.beginGame($('#gameid').text());
        $('#startButton').addClass('hidden');
    });

});

function hideJoinInputs() {
    $('#playerNameInput').addClass('hidden');
    $('#partynameinput').addClass('hidden');
    $('#join').addClass('hidden');
};

hub.client.showPartyName = function (partyName) {
    $('#partyName').text(partyName);
    $('#partyName').removeClass('hidden');
};

hub.client.showPlayerName = function (playerName) {
    $('#playerName').text(playerName);
    $('#playerName').removeClass('hidden');
};

hub.client.showOtherPlayer = function (otherPlayerName, otherPlayerID) {
    $('#otherplayers').append('<div id=' + otherPlayerID + ' class="otherplayercontainer col-md-6">' +
        '<div>' + otherPlayerName + '</div>' +
        '<div class="role">Role: Unknown</div>' +
        '<div class="specialist">Specialist: Unknown</div>' +
    '</div>');
};

hub.client.showStartButton = function () {
    $('#startButton').removeClass('hidden')
};

hub.client.setPlayerID = function (playerID) {
    $('#playerid').text(playerID);
};

hub.client.setPartyID = function (gameID) {
    $('#gameid').text(gameID);
};

hub.client.showRole = function (isTraitor) {
    if (isTraitor) {
        $('#rolebox').text("traitor");
    }
    else {
        $('#rolebox').text("rebel");
    }
};
hub.client.showSpecialist = function (specialist) {
    $('#specialist').text(specialist);
};

hub.client.showOtherSpecialist = function (specialist, playerID) {
    $('#' + playerID).find('.specialist').text(specialist);

};

hub.client.showGameBegun = function () {
    $('#gamestate').text("Game begun");
};

hub.client.showOtherRole = function (playerID, role) {
    $('#' + playerID).find('.role').text(role);
    if (role == 'traitor') {
        $('#' + playerID).addClass('traitor');
    }
    if (role == 'rebel') {
        $('#' + playerID).addClass('rebel');
    }
}


hub.client.takeTurn = function () {
    setGameState("Your Turn");
    var singleClickSpecialists = ["investigator", "thief", "analyst", "confirmer", "revealer"];
    var multiClickSpecialists = ["reassinger", "signaller"];
    console.log("take turn reached");

    if (singleClickSpecialists.includes(getPlayerSpecialist() )) {
        console.log("single click specialist");
        $('.otherplayercontainer').on('click', function () {
            var selectedID = $(this).attr('id');
            console.log("single click specialist click detected");
            hub.server.takeSingleAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), selectedID);
            $('.otherplayercontainer').off('click');
            setGameState("waiting for other players");
        });
    }
    else if (getPlayerSpecialist() == "signaller") {
        console.log("signaller");
        console.log(getPlayerRole());
        if (getPlayerRole() == "rebel") {
            $('.otherplayercontainer').on('click', function () {
                var selectedID = $(this).attr('id');
                setGameState("waiting for other players");
                hub.server.takeSingleAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), selectedID);
                $('.otherplayercontainer').off('click');
            });
        }
        else {
            rolecontainers = $('.role')
            for (role in rolecontainers) {
                if ($(role).text() == "traitor") {
                    $(role).parent().on('click', function () {
                        var selectedID = $(this).attr('id');
                        setGameState("waiting for other players");
                        hub.server.takeSingleAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), selectedID);
                        $('.otherplayercontainer').off('click');
                    });
                }
            }
        }
    }
    else if (getPlayerSpecialist() == "reassigner") {
        console.log("reassigner");
        $('#takeactionbutton').removeClass('hidden');
        var swapIDArray = [];

        $('.otherplayercontainer').on('click', function () {
            swapIDArray.push($(this).attr('id'));
            this.setAttribute("class", "otherplayercontainer selectedplayer")
            console.log
            if (getPlayerRole() == "rebel") {
                if (swapIDArray.length == 2) {
                    hub.server.takeReassignRebelAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), swapIDArray[0], swapIDArray[1]);
                    $('.otherplayercontainer').off('click');
                    setGameState("waiting for other players");
                }
            }
            else {
                hub.server.takeReassignTraitorAction(getPartyName(), getPlayerID(), getPlayerSpecialist(), swapIDArray[0]);
                setGameState("waiting for other players");
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

hub.client.addClickToVoteHandlers = function () {
    $('.otherplayercontainer').on('click', function () {
        var selectedID = $(this).attr('id');
        hub.server.addVote(getPlayerID(), selectedID);
        $('.otherplayercontainer').off('click');
        setGameState("waiting for other players");
    });
}

hub.client.setGameStateFromServer = function (message) {
    $('#gamestate').text(message);
}

hub.client.addEndGameButton = function () {
    $('#endButton').removeClass('hidden');
    $('#endButton').click(function () {
        hub.server.endGame(getGameID());
    });
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

getGameID = function () {
    return $('#gameid').text();
};

setGameState = function (message) {
    $('#gamestate').text(message);
};
