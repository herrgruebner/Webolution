var hub = $.connection.oneNightWebolutionHub;

$.connection.hub.start().done(function () {
    $('#join').click(function () {
        hub.server.addPlayerToParty($('#playerNameInput').val(), $('#partyname').val());

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

hub.client.showGameBegun = function () {
    $('#gamestate').text("Game begun");
};

hub.client.showOtherRole = function (playerID, role) {
    $('#playerID').find('#role').text(role);
}

hub.client.takeTurn = function () {
    $('#gamestate').text("Your Turn");
    var singleActionSpecialists = ["investigator", "signaller", "thief", "analyst", "confirmer", "revealer"];
    
    if (getPlayerSpecialist() in singleActionSpecialists) {
        $('#otherplayercontainer').on('click', function () {
            var selectedID = $(this).attr('id');

            hub.server.takeSingleAction(getPlayerID(), getPlayerSpecialist(), selectedID);
        });
    };
    
};

getPlayerID = function () {
    return $('#playerid').text();
};

getPlayerSpecialist = function (){
    return $('#specialist').text();
}