class TraitifyCC {
    constructor(model, traitify, isAuthenticated) {
        this.model = model;
        this.traitify = traitify;
        this.initialize();
        this.isAuthenticated = isAuthenticated;
    }

    initialize() {
        this.setUrl(this.model.assessmentId);
        var assessmentId = this.model.assessmentId;
        this.traitify.setHost(this.model.host);
        this.traitify.setPublicKey(this.model.publicKey);
        var assessment = this.traitify.ui.component();
        assessment.on("SlideDeck.Finished", function () {
            var url = '/traitify/complete/' + assessmentId;
            $("#traitifyInstructions").hide();
            $.ajax({
                url: url,
                type: 'POST',
                error: function () {
                    ToastService.error('Oops! Something unexpected happened, and we are looking into it.')
                },
                success: function (results) {
                    if (results.isAuthenticated) {
                        $('.traitify-modal').modal();
                        assessment.render("Results");
                    } else {
                        $("#Email").val(results.email);
                        $("#traitify-hidden-signup-container").show()
                        $("#resultFooter").show();
                        assessment.render("PersonalityBlend");
                    }
                    assessment.target("#traitify");
                }
            });
        });
        assessment.target("#traitify");
        if (this.model.isComplete) {
            assessment.assessmentID(this.model.assessmentId);
            $("#traitifyInstructions").hide();
            if (this.model.isAuthenticated || this.model.isRegistered) {
                assessment.render("Results");
            } else {
                $("#Email").val(this.model.email);
                $("#traitify-hidden-signup-container").show()
                assessment.render("PersonalityBlend");
                $("#resultFooter").show();
            }
        } else {
            assessment.assessmentID(assessmentId);
            assessment.allowFullscreen();
            assessment.render("SlideDeck");
        }

        $("#SignUpComponent").submit(function (e) {
            $("body").prepend("<div class=\"overlay\" id=\"SignUpOverlay\"><div id=\"loading-img\" ></div></div>");
            $("#SignUpOverlay").show();
            e.preventDefault();
            var agreedTos = $('#SignUpComponent #termsAndConditionsCheck').is(':checked');
            $('#SignUpComponent #termsAndConditionsCheck').toggleClass('invalid', !agreedTos);
            if ($("#SignUpComponent #Email").val() && $("#SignUpComponent #Password").val() && $("#SignUpComponent #ReenterPassword").val() && agreedTos) {
                if ($("#SignUpComponent #Password").val() !== $("#SignUpComponent #ReenterPassword").val()) {
                    $("#SignUpOverlay").remove();
                    ToastService.error("The passwords you have entered do not match.", 'Whoops...');
                } else {
                    $.ajax({
                        type: "POST",
                        url: "/traitify/createaccount",
                        data: $(this).serialize()
                    }).done(res => {
                        $("#traitify-hidden-signup-container").hide()
                        $("#resultFooter").hide();
                        $('.traitify-modal').modal();
                    }).fail(res => {
                        var errorText = "Unfortunately, there was an error with your submission. Please try again later.";
                        if (res.responseJSON.description != null)
                            errorText = res.responseJSON.description;
                        ToastService.error(errorText, 'Whoops...');
                    }).always(() => {
                        $("#SignUpOverlay").remove();
                    });
                }
            } else {
                ToastService.error("Please enter information for all sign-up fields and try again.");
                $("#SignUpOverlay").remove();
            }
        });
    }

    setUrl(assessmentId) {
        var newurl = window.location.protocol + "//" + window.location.host + "/traitify/" + assessmentId;
        window.history.pushState({
            path: newurl
        }, '', newurl);
    }
}