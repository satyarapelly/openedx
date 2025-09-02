// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function renderCheckoutSession(session, stripePublishableKey) {

    console.log("Stripe checkout session:");
    console.log("-------------------------------------------------------------");
    console.log("id                : " + session.id);
    console.log("mode              : " + session.mode);
    console.log("amount_total      : " + session.amount_total);
    console.log("payment_intent_id : " + session.payment_intent_id);
    console.log("payment_status    : " + session.payment_status);
    console.log("-------------------------------------------------------------");

    var stripe = Stripe(stripePublishableKey);
    var stripeResponse = stripe.redirectToCheckout({ sessionId: session.id });
}

function renderStripeCardForm(stripeClientSecret, stripePublishableKey, ru, rx) {
    var stripe = Stripe(stripePublishableKey);
    var elements = stripe.elements();
    var style = {
        base: {
            color: "#32325d",
        }
    }
    var card = elements.create("card", { style: style });
    card.mount("#card-element");

    // Listen to change events and show any errors
    card.on('change', function (event) {
        var displayError = document.getElementById('card-errors');
        if (event.error) {
            displayError.textContent = event.error.message;
        } else {
            displayError.textContent = '';
        }
    });

    // Hook up Pay button
    var form = document.getElementById('payment-form');

    /*
    form.addEventListener('submit', async function (ev) {
        ev.preventDefault();

        if (document.getElementById('card-radio').checked) {

            // Credit Card transaction

            stripe.confirmCardPayment(stripeClientSecret, {
                payment_method: {
                    card: card,
                    billing_details: {
                        name: 'Jenny Rosen'
                    }
                }
            }).then(function (result) {
                if (result.error) {
                    // Show error to your customer (e.g., insufficient funds)
                    console.log(result.error.message);
                    window.location.replace(rx);
                } else {
                    // The payment has been processed!
                    if (result.paymentIntent.status === 'succeeded') {
                        console.log('Payment Intent succeeded');
                        setTimeout(
                            window.location.replace(ru),
                            7000);
                        // Show a success message to your customer
                        // There's a risk of the customer closing the window before callback
                        // execution. Set up a webhook or plugin to listen for the
                        // payment_intent.succeeded event that handles any business critical
                        // post-payment actions.
                    }
                }
            });
        }
        else {

            // CheckWire transaction
            const result = await fetch({
                url: '/sdfsfsf',
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
            });

            alert(await result.text());
        }
    });*/
}


function checkoutPageOnLoad() {

    // When the page loads, no PIs are selected.  Select the first PI.  This must be called 
    // after hooking up the even handler above.
    $('input:radio[name=piList]:first').click();
}

function checkoutPagePayButtonOnClick(orderId) {
    // Disable Pay button
    $('#payButton').removeClass('btn-primary').addClass('btn-disabled');

    // Show Pay spinner
    $('#paySpinner').css('display', 'inline-block');

    var selectedPiId = $('input:radio[name=piList]:checked').attr('id');
    var submitUrl = '/Checkout/PayWithPIOnFile/?orderId=' + orderId + "&piId=" + selectedPiId;
    location.href = submitUrl;
}

function ordersPageOnLoad() {
    // Highlight orders based on their class names.  Typically, only 1 order (the most recent one)
    // will have the highlight<Status> class.
    $(".highlightSucceeded").effect("highlight", { color: '#A0F0A0' }, 2000);
    $(".highlightFailed").effect("highlight", { color: '#F0A0A0' }, 2000);
    $(".highlightPending").effect("highlight", { color: '#F0F0A0' }, 2000);

    // Setup SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/ordersHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    async function start() {
        try {
            await connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.log(err);
            setTimeout(start, 5000);
        }
    };

    connection.on("OrderStateChanged", () => {
        window.location.reload(true);
    });

    connection.onclose(async () => {
        await start();
    });

    start();
}
