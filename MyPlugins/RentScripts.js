var Sdk = window.Sdk || {};
(
    function () {
        this.comparePickupReturnDates = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var reservedPickup = new Date(formContext.getAttribute("new_reservedpickup").getValue());
            var reservedHandover = new Date(formContext.getAttribute("new_reservedhandover").getValue());
            if (reservedPickup > reservedHandover) {
                formContext.getControl("new_reservedhandover").setNotification("Return Time cannot be earlier than Pickup Time.", "returndatemsg");
                formContext.ui.setFormNotification("Please, select correct Return Time.", "INFO", "returninfo")
            } else {
                formContext.getControl("new_reservedhandover").clearNotification("returndatemsg");
                formContext.ui.clearFormNotification("returninfo");
            }
        }

        this.checkPickupDate = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var reservedPickup = new Date(formContext.getAttribute("new_reservedpickup").getValue());
            var today = new Date();
            if (reservedPickup >= today) {
                formContext.getControl("new_reservedpickup").clearNotification("pickupdatemsg");
                formContext.ui.clearFormNotification("pickupinfo");
            } else {
                formContext.getControl("new_reservedpickup").setNotification("Pickup Time cannot be earlier than Today.", "pickupdatemsg");
                formContext.ui.setFormNotification("Please, select correct Pickup Time.", "INFO", "pickupinfo")
            }
        }

        this.checkOnRentingStatus = function (executionContext) {
            const RENTING = 279640002;
            var formContext = executionContext.getFormContext();
            var statusCode = formContext.getAttribute("statuscode").getValue();
            var isPaid = formContext.getAttribute("new_paid").getValue();
            if (statusCode === RENTING && !isPaid) {
                formContext.ui.setFormNotification("Car rent is not yet paid.Car cannot be rented", "WARNING", "paidinfo");
            } else {
                formContext.ui.clearFormNotification("paidinfo");
            }
        }
    }
).call(Sdk);