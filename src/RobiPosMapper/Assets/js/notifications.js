function SuccessNotification(UserMessage) {
    $.pnotify({
        type: 'success',
        title: 'Success',
        text: UserMessage,
        icon: 'picon icon16 iconic-icon-check-alt white',
        opacity: 0.95,
        history: false,
        sticker: false
    });
}

//for general purpose alert in red
function AlertNotification(Title, UserMessage) {
    $.pnotify({
        type: 'error',
        title: Title,
        text: UserMessage,
        icon: 'picon icon24 typ-icon-cancel white',
        opacity: 0.95,
        history: false,
        sticker: false
    });
}

//error
function ErrorNotification(UserMessage) {
    $.pnotify({
        type: 'error',
        title: 'Error !',
        text: UserMessage,
        icon: 'picon icon24 typ-icon-cancel white',
        opacity: 0.95,
        history: false,
        sticker: false
    });
}