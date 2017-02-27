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

hub.client.showOtherPlayer = function (otherPlayerName) {
    $('#otherplayers').append('<div>' + otherPlayerName + '</div>');
};

hub.client.showStartButton = function () {
    $('startButton').removeClass('hidden');
};

hub.client.setPlayerID = function (playerID) {
    $('#playerid').text(playerID);
}

hub.client.setPartyID = function (gameID) {
    $('#gameid').text(gameID);
}

getPlayerID = function () {
    return $('#playerid').text();
};