var hub = $.connection.oneNightWebolutionHub;


$.connection.hub.start().done(function () {
    $('#join').click(function () {
        hub.server.addPlayerToParty($('#playerNameInput').val(), $('#partyname').val());

    });

    $('#startButton').click(function () {
        hub.server.BeginGame($('#partyName').val());
    })
});

hub.client.showPartyName = function (partyName) {
    $('#partyName').text(partyID);
};

hub.client.showPlayerName = function (playerName) {
    $('#playerName').text(partyID);
};

hub.client.showOtherPlayer = function (otherPlayerName) {
    $('#otherplayers').append('<div>' + otherPlayerName + '</div>');
};

hub.client.showStartButton = function () {
    $('startButton').removeClass('hidden');
}