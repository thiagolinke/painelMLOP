/**
 * This class deal with login tasks: password expired, login, change password
 * @author Fidelis lalasdflasdlfksd
 */
function Login(){

	//HTML conent elements declarations	 
	this.__divContainer; //principal div where we will put inside all HTML created
	this.__divLogo;
	this.__divWrapMessage; //div that will wrap message elements
	this.__divMessageContent //div containing message elements, such button, icon text message
	this.__divWrapContent; //div that will wrap current content, that can be loginContent or expiredPasswordContent
	this.__divLoginContent;
	this.__divExpiredPasswordContent;
	this.__divForgotPassword;


	//HTML control elements declarations
	//login content
	this.__submitButton;
	this.__changePasswordButton;
	this.__passwordInput;
	this.__userNameInput;
	this.__rememberMeInput;
	this.__forgotPasswordInput;
	//expired password content
	this.__newPasswordInput1;
	this.__newPasswordInput2;
	this.__oldPasswordInput;

	
	//control variables
	this.__showForgotPasswordButton; //show forgot password button on interface
	this.__doLoginRequestOnEnterPressed;//do login request on enter pressed, if false do change password request instead
	//ajaxHandler object
	this.__ajaxhandler;

	this.initiate();
}

/**
 * constant object with all message CSS classes
 * @type {Object}
 */
Login.MESSAGE_CLASSES = Object.freeze({
	error : "alert alert-danger fade in widget-inner",
	warning:"alert alert-success fade in widget-inner",
	success:"alert alert-success fade in widget-inner",
});

/**
 * constant object with all icon CSS classes
 * @type {Object}
 */
Login.ICON_CLASSES = Object.freeze({
	error : "fa fa-warning",
	warning:"fa fa-warning",
	success:"fa fa-check",
});



/**
 * initiate login HTML content with default login panel
 */
Login.prototype.initiate = function (){

	var v_body = $(".body-login")[0];	
	//create ajax object to perform requests
	this.__ajaxhandler = new LoginAjaxHandler(this);

	this.__divContainer = $(".login-wrapper")[0];
	this.__divWrapContent = ELEMENT_FACTORY.createElement("div",{ __parentJS: this, onkeypress: this.onKeyPressAction,role:"form"},{});
	this.__createLogo();
	this.__createMessageContent();
	this.__createLoginContent();
	this.__createExpiredPasswordContent();
	this.__createForgotPasswordContent();

	//append HTML DOM objects inside __divContainer	
	this.__divWrapContent.appendChild(this.__divLoginContent);
	this.__divContainer.appendChild(this.__divLogo);	
	this.__divContainer.appendChild(this.__divWrapContent);
	this.__divContainer.appendChild(this.__divForgotPassword);
	this.__divContainer.appendChild(this.__divWrapMessage);

	this.__showForgotPasswordButton = true;
	this.__doLoginRequestOnEnterPressed = true;

	//set remember me according with cookie value
	this.__rememberMeInput.checked = GetCookie("STAY_SIGNED") == "True";
	if (GetCookie("USERNAME") != ""){
		this.isThereSomeUserLoggedRequest();

	}

};

/**
 * create logo HTML tags and put it in __divLogo
 */
Login.prototype.__createLogo = function (){

	var v_imageLogo = ELEMENT_FACTORY.createElement("img", { src: "images/new_interface/logos/logo-remopt-login.png" },{}) 
	this.__divLogo = ELEMENT_FACTORY.createElement("div", {className: "wrap-logo-login"},{})

 	this.__divLogo.appendChild(v_imageLogo);

};

/**
 * create login panel content and put it in __divWrapContent
 */
Login.prototype.__createLoginContent = function (){
	
	var v_divPanelHeading, v_divPanelBody;

	//wrap elements inside v_divPanelBody
	var v_divWrapContentPanelBody, v_divWrapUsername, v_divWrapPassword, v_divWrapActions, v_divWrapRememberMe, v_divWrapSubmitButton;
	//inputs and labels inside v_divPanelBody
	var v_labelUsername, v_iconUsername, v_labelPassword, v_iconPassword, v_divRememberMe, v_labelRememberMe, v_textRememberMe
	//create panel heading
	v_divPanelHeading = ELEMENT_FACTORY.createElement("div",{className:"panel-heading"},{});
	v_divPanelHeading.innerHTML = '<h6 class="panel-title"><i class="fa fa-user"></i> User login</h6>';

	//create panel body
	v_divPanelBody = ELEMENT_FACTORY.createElement("div",{className:"panel-body"},{});
	v_divWrapContentPanelBody = ELEMENT_FACTORY.createElement("div",{className:"wrap-login"},{});
	
	//div elements in panel body
	v_divWrapUsername = ELEMENT_FACTORY.createElement("div",{className:"form-group has-feedback"},{});
	v_divWrapPassword = ELEMENT_FACTORY.createElement("div",{className:"form-group has-feedback"},{});
	v_divWrapActions = ELEMENT_FACTORY.createElement("div",{className:"row form-actions"},{});
	v_divWrapRememberMe = ELEMENT_FACTORY.createElement("div",{className:"col-xs-6"},{});
	v_divWrapSubmitButton = ELEMENT_FACTORY.createElement("div",{className:"col-xs-6"},{});

	//others elements in panel body
	v_labelUsername = ELEMENT_FACTORY.createElement("label",{textContent: "Username"},{});
	v_iconUsername = ELEMENT_FACTORY.createElement("i",{className:"fa fa-user form-control-feedback"},{});
	v_labelPassword = ELEMENT_FACTORY.createElement("label",{textContent: "Password"},{});
	v_iconPassword = ELEMENT_FACTORY.createElement("i",{className:"fa fa-lock form-control-feedback"},{});
	v_divRememberMe = ELEMENT_FACTORY.createElement("div",{className:"checkbox"},{});
	v_labelRememberMe = ELEMENT_FACTORY.createElement("label",{},{});
	v_textRememberMe = document.createTextNode("Keep Me Logged In"); //André Teixeira on 2015-10-08: The real functionality here is to keep the user logged in (we will change the variable names in the future)

	//create all inputs and buttons
	this.__submitButton= ELEMENT_FACTORY.createElement(
		"button",
		{//properties
			__parentJS: this,
			type: "submit",
			className: "btn btn-warning pull-right",
			textContent: "Sign in",
			onclick: this.onclickSubmitButton
		},
		{/*CSS*/}
	);
	this.__passwordInput = ELEMENT_FACTORY.createElement("input", {type: "password", className: "form-control", placeholder: "password"}, {});
	this.__userNameInput = ELEMENT_FACTORY.createElement("input", {type: "text", className: "form-control", placeholder: "username"}, {});;;
	this.__rememberMeInput = ELEMENT_FACTORY.createElement("input", {type: "checkbox", className: "styled"}, {});;


	//div that wrap all login content, so if we need to change 	__divWrapContent to login content again,
	//we just set its innerHTML = '' and appendChild(this.__divLoginContent)
	this.__divLoginContent = ELEMENT_FACTORY.createElement("div",{className: "panel panel-default"},{});

	//setting hierarchy	
	this.__divLoginContent.appendChild(v_divPanelHeading);
	this.__divLoginContent.appendChild(v_divPanelBody);

	v_divPanelBody.appendChild(v_divWrapContentPanelBody);
	v_divWrapContentPanelBody.appendChild(v_divWrapUsername);	
	v_divWrapContentPanelBody.appendChild(v_divWrapPassword);	
	v_divWrapContentPanelBody.appendChild(v_divWrapActions);	

	v_divWrapUsername.appendChild(v_labelUsername);
	v_divWrapUsername.appendChild(this.__userNameInput);
	v_divWrapUsername.appendChild(v_iconUsername);

	v_divWrapPassword.appendChild(v_labelPassword);
	v_divWrapPassword.appendChild(this.__passwordInput);
	v_divWrapPassword.appendChild(v_iconPassword);

	v_divWrapActions.appendChild(v_divWrapRememberMe);
	v_divWrapActions.appendChild(v_divWrapSubmitButton);

	v_divWrapRememberMe.appendChild(v_divRememberMe);
	v_divRememberMe.appendChild(v_labelRememberMe);
	v_labelRememberMe.appendChild(this.__rememberMeInput);
	v_labelRememberMe.appendChild(v_textRememberMe);

	v_divWrapSubmitButton.appendChild(this.__submitButton);
};

/**
 * create warning message default contents
 */
Login.prototype.__createMessageContent = function (){

	var v_closeMessageButton = ELEMENT_FACTORY.createElement(
	 	"button",
	 	{
	 		__parentJS:this,
	 		type:"button",
	 		className: "close",
	 		onclick: this.onclickCloseMessageButton,
	 		textContent: "x"
	 	},
 		{}
	);
 var v_icon = ELEMENT_FACTORY.createElement("icon",{},{});
 var v_textMessage = document.createTextNode("default message");

	this.__divWrapMessage = ELEMENT_FACTORY.createElement("div",{},{});

	this.__divMessageContent = ELEMENT_FACTORY.createElement("div",{},{});

	//save reference to icon object and textMessage
	this.__divMessageContent.icon = v_icon;
	this.__divMessageContent.textMessage = v_textMessage;

	//set hierarchy
	//this.__divMessageContent.appendChild(v_closeMessageButton);
	this.__divMessageContent.appendChild(v_icon);
	this.__divMessageContent.appendChild(v_textMessage);
};


Login.prototype.__createForgotPasswordContent = function (){

	var v_anchorElement = ELEMENT_FACTORY.createElement("a",{title:"Forgot your password?", textContent: "Forgot my password"},{});

	//creating forgotPassword panel
	//<a href="login-forgot.php" title="Forgot your password?" target="_self"> Forgot my password</a>
	this.__divForgotPassword = ELEMENT_FACTORY.createElement("div",{ __parentJS: this, className:"panel-forgot", onclick: this.onclickForgotPassword},{});
	this.__divForgotPassword.appendChild(v_anchorElement);

};

/**
 * Change div and icon classes name, so we will properly style according with type message: 'error', 'success' or 'warning'.
 * 
 * @param	{String} p_classType - String containing the class type that we stored in MESSAGE_CLASSES constant object, 
 * it can be: 'error', 'success' or 'warning'. 
 * @param {String} p_textMessage - String containing the message.
 */
Login.prototype.__changeDivMessageContentClass = function (p_classType, p_textMessage){
	//check if exists this type of class
	if (Login.MESSAGE_CLASSES[p_classType]){
		this.__divMessageContent.className = Login.MESSAGE_CLASSES[p_classType];
	}

	//check if exists this type of class
	if (Login.ICON_CLASSES[p_classType]){
		this.__divMessageContent.icon.className = Login.ICON_CLASSES[p_classType];
	}

	this.__divMessageContent.textMessage.textContent = p_textMessage;

};

/**
 * Clean divWrapMessage
 */
Login.prototype.hideMessage = function (){
	this.__divWrapMessage.innerHTML = "";
};
 
/**
 * Set divMessage content inside divWrapMessage.
 */
Login.prototype.showMessage = function (){
	this.__divWrapMessage.appendChild(this.__divMessageContent);
};
/**
 * set showForgotPasswordButton variable
 * @param {Boolean} p_showForgotPassword - true or false, this value will be set in showForgotPasswordButton
 */
Login.prototype.__setShowForgotPasswordButton = function (p_showForgotPassword){
	if (p_showForgotPassword == undefined) p_showForgotPassword = true;

	this.__showForgotPasswordButton = p_showForgotPassword;
};

Login.prototype.__emptyFields = function(){

	this.__newPasswordInput1.value = "";
	this.__newPasswordInput2.value = ""
	this.__oldPasswordInput.value = ""
	this.__userNameInput.value = ""
	this.__passwordInput.value = ""
};

/**
 * Change __divWrapContent content
 * @param {String} [p_panelToShow = login] - string content what panel to show: "login" or "expiredPassword". Default "login".
 */
Login.prototype.__changePanelContent = function (p_panelToShow){
	if (p_panelToShow == undefined) p_panelToShow = "login";

	//change panel content
	this.__divWrapContent.innerHTML = "";
	if (p_panelToShow == "login"){
		this.__divWrapContent.appendChild(this.__divLoginContent);
		this.__doLoginRequestOnEnterPressed = true;
	}
	else{
		this.__divWrapContent.appendChild(this.__divExpiredPasswordContent);
		this.__doLoginRequestOnEnterPressed = false;	
	}

	//attach forgot button if necessary
	if (this.__showForgotPasswordButton){
		this.__divContainer.appendChild(this.__divForgotPassword);
	}
	else{
		this.__divContainer.removeChild(this.__divForgotPassword);
	}

};

/**
 * create expiredPassword panel content.
 */
Login.prototype.__createExpiredPasswordContent = function (){

	var v_divPanelHeading, v_divPanelBody;

	//wrap elements inside v_divPanelBody
	var v_divWrapContentPanelBody, v_divWrapOldPassword, v_divWrapNewPassword1, v_divWrapNewPassword2, v_divWrapActions, v_divWrapEmptyCol, v_divWrapChangePasswordButton;
	//inputs and labels inside v_divPanelBody
	var v_labelOldPassword, v_iconOldPassword, v_labelNewPassword1, v_iconNewPassword1, v_labelNewPassword2, v_iconNewPassword2;
	//create panel heading
	v_divPanelHeading = ELEMENT_FACTORY.createElement("div",{className:"panel-heading"},{});
	v_divPanelHeading.innerHTML = '<h6 class="panel-title"><i class="fa fa-user"></i> Change Password </h6>';

	//create panel body
	v_divPanelBody = ELEMENT_FACTORY.createElement("div",{className:"panel-body"},{});
	v_divWrapContentPanelBody = ELEMENT_FACTORY.createElement("div",{className:"wrap-login"},{});
	
	//div elements in panel body
	v_divWrapOldPassword = ELEMENT_FACTORY.createElement("div",{className:"form-group has-feedback"},{});
	v_divWrapNewPassword1 = ELEMENT_FACTORY.createElement("div",{className:"form-group has-feedback"},{});
	v_divWrapNewPassword2 = ELEMENT_FACTORY.createElement("div",{className:"form-group has-feedback"},{});
	v_divWrapActions = ELEMENT_FACTORY.createElement("div",{className:"row form-actions"},{});
	v_divWrapEmptyCol = ELEMENT_FACTORY.createElement("div",{className:"col-xs-6"},{});
	v_divWrapChangePasswordButton = ELEMENT_FACTORY.createElement("div",{className:"col-xs-6"},{});

	//others elements in panel body
	v_labelOldPassword = ELEMENT_FACTORY.createElement("label",{textContent: "Old Password"},{});
	v_iconOldPassword = ELEMENT_FACTORY.createElement("i",{className:"fa fa-lock form-control-feedback"},{});
	v_labelNewPassword1 = ELEMENT_FACTORY.createElement("label",{textContent: "New Password"},{});
	v_iconNewPassword1 = ELEMENT_FACTORY.createElement("i",{className:"fa fa-lock form-control-feedback"},{});
	v_labelNewPassword2 = ELEMENT_FACTORY.createElement("label",{textContent: "Retype New Password"},{});
	v_iconNewPassword2 = ELEMENT_FACTORY.createElement("i",{className:"fa fa-lock form-control-feedback"},{});

	//creating forgotPassword panel
	this.__divForgotPassword = ELEMENT_FACTORY.createElement("div",{className:"panel-forgot"},{});

	//create all inputs and buttons
	this.__changePasswordButton= ELEMENT_FACTORY.createElement(
		"button",
		{//properties
			__parentJS: this,
			type: "button",
			className: "btn btn-warning pull-right",
			textContent: "Change Password",
			onclick: this.onclickChangePassword
		},
		{/*CSS*/}
	);
	this.__oldPasswordInput = ELEMENT_FACTORY.createElement("input", {type: "password", className: "form-control", placeholder: "password"}, {});
	this.__newPasswordInput1 = ELEMENT_FACTORY.createElement("input", {type: "password", className: "form-control", placeholder: "password"}, {});
	this.__newPasswordInput2 = ELEMENT_FACTORY.createElement("input", {type: "password", className: "form-control", placeholder: "password"}, {});


	//div that wrap all login content, so if we need to change 	__divWrapContent to login content again,
	//we just set its innerHTML = '' and appendChild(this.__divExpiredPasswordContent)
	this.__divExpiredPasswordContent = ELEMENT_FACTORY.createElement("div",{className: "panel panel-default"},{});

	//setting hierarchy
	
	this.__divExpiredPasswordContent.appendChild(v_divPanelHeading);
	this.__divExpiredPasswordContent.appendChild(v_divPanelBody);

	v_divPanelBody.appendChild(v_divWrapContentPanelBody);
	v_divWrapContentPanelBody.appendChild(v_divWrapOldPassword);	
	v_divWrapContentPanelBody.appendChild(v_divWrapNewPassword1);
	v_divWrapContentPanelBody.appendChild(v_divWrapNewPassword2);	
	v_divWrapContentPanelBody.appendChild(v_divWrapActions);	

	v_divWrapOldPassword.appendChild(v_labelOldPassword);
	v_divWrapOldPassword.appendChild(this.__oldPasswordInput);
	v_divWrapOldPassword.appendChild(v_iconOldPassword);

	v_divWrapNewPassword1.appendChild(v_labelNewPassword1);
	v_divWrapNewPassword1.appendChild(this.__newPasswordInput1);
	v_divWrapNewPassword1.appendChild(v_iconNewPassword1);

	v_divWrapNewPassword2.appendChild(v_labelNewPassword2);
	v_divWrapNewPassword2.appendChild(this.__newPasswordInput2);
	v_divWrapNewPassword2.appendChild(v_iconNewPassword2);

	v_divWrapActions.appendChild(v_divWrapEmptyCol);
	v_divWrapActions.appendChild(v_divWrapChangePasswordButton);
	
	v_divWrapChangePasswordButton.appendChild(this.__changePasswordButton);
	
};

/**
 * Do "is some user logged" request.
 */
Login.prototype.isThereSomeUserLoggedRequest = function (){
	var v_url = "doLogin.ashx";	
	var v_data= {};

	v_data.action = "isThereSomeUserLogged";	

	this.__ajaxhandler.doGenericJsonAjaxRequest(v_url, v_data);

};

/**
 * Callback function for isThereSomeUserLoggedRequest method
 * @param  {Object} p_data - data containing information about isThereSomeUserLoggedRequest action
 */
Login.prototype.isThereSomeUserLoggedCallBack = function (p_data){
	if (p_data.isThereSomeUserLogged){
		window.open('default.aspx', '_self');	
	}
};

/**
 * Do login request.
 */
Login.prototype.loginRequest = function (){
	var v_url = "doLogin.ashx";	
	var v_data= {};

	v_data.action = "login";
	v_data.u = this.__userNameInput.value;
	v_data.p = encriptPassword(v_data.u, this.__passwordInput.value);
	v_data.rememberMe = this.__rememberMeInput.checked;


	this.__ajaxhandler.doGenericJsonAjaxRequest(v_url, v_data);

};


/**
 * Callback function for loginRequest method
 * @param  {Object} p_data - data containing information about login action
 */
Login.prototype.loginRequestCallBack = function (p_data){
	
	if (p_data.expiredPassword){
		this.printErrorMessage(" Your password is expired, please choose a new one.")
		this.__changePanelContent("expiredPassword");

	}
	else{
		window.open('default.aspx', '_self');	
	}
	
};

/**
 * Do password expired request.
 */
Login.prototype.expiredPasswordRequest = function (){

	var v_validateErrorMessage = this.__validateExpiredPasswordFields();
	var v_url = "doLogin.ashx";
	var v_data= {};

	if (v_validateErrorMessage != ""){
		this.printErrorMessage(v_validateErrorMessage);
		return ; 
	}

	v_data.action = "expiredPassword";
	v_data.u = this.__userNameInput.value;
	v_data.pOld = encriptPassword(v_data.u, this.__oldPasswordInput.value);
	v_data.pNew1 = encriptPassword(v_data.u, this.__newPasswordInput1.value);
	v_data.pNew2 = encriptPassword(v_data.u, this.__newPasswordInput2.value);

	this.__ajaxhandler.doGenericJsonAjaxRequest(v_url, v_data);
};

/**
 * callback function for expiredPasswordRequest method
 * @param  {Object} p_data - data containing information about password expired action
 */
Login.prototype.expiredPasswordCallBack = function (p_data){
	
	if (p_data.passwordChanged){
		this.printSuccessMessage(" Your password was successfully updated.");
		this.__emptyFields();
		this.__changePanelContent("login")
	}
	else{
		this.printErrorMessage(" Wrong old password. Try again.");
	}

};

/**
 * Do forgot password request.
 * @return {[type]} [description]
 */
Login.prototype.forgotPasswordRequest = function (){
	var v_url = "doLogin.ashx";
	var v_data= {};

	v_data.action = "forgotPassword";
	v_data.u = this.__userNameInput.value;
	
	if (!v_data.u){
		this.printErrorMessage(" Please insert your username before you request your password. ");
	}
	else{
		this.__ajaxhandler.doGenericJsonAjaxRequest(v_url, v_data);	
	}	

};

/**
 * callback function for forgotPasswordRequest method
 * @param  {Object} p_data - data containing information about forgot password action
 */
Login.prototype.forgotPasswordCallBack = function (p_data){
	
	if (p_data.sendedEmail){
		this.printSuccessMessage(" A new password was sent to your e-mail.");
		this.__setShowForgotPasswordButton(false); //to only request a new password one time
		this.__emptyFields();
		this.__changePanelContent("login"); 
	}
	else{
		this.printErrorMessage(" An Error occurred when we were trying to send your new password to your email: " + p_data.emailMessage);
	}
};

Login.prototype.onEnterPressAction = function (){
	if (this.__doLoginRequestOnEnterPressed){
		this.loginRequest();
	}
	else{
		this.expiredPasswordRequest();
	}
};

Login.prototype.__validateExpiredPasswordFields = function (){
	var v_errorMessage = "";

	if (this.__oldPasswordInput.value == "" || this.__newPasswordInput1.value == ""  || this.__newPasswordInput2.value == "") {
		v_errorMessage = " Missing Fields.";
	}
	else if (this.__newPasswordInput1.value != this.__newPasswordInput2.value) {
		v_errorMessage = " Invalid password confirmation.";
	}
	else if (this.__newPasswordInput1.value == this.__oldPasswordInput.value){
		v_errorMessage = " The new password must be different from the current password.";	
	}
	else if (this.__newPasswordInput1.value.length < 6 ){
		v_errorMessage = " The new password must have at least 6 characters.";	
	}

	return v_errorMessage;

};


Login.prototype.printErrorMessage = function (p_message){
	this.hideMessage();
	this.__changeDivMessageContentClass("error", p_message);
	this.showMessage();
};


Login.prototype.printSuccessMessage = function (p_message){
	this.hideMessage();
	this.__changeDivMessageContentClass("success", p_message);	
	this.showMessage();
};

/**
 * STATIC EVENT HANDLERS METHODS
 */

Login.prototype.onclickSubmitButton = function (){
	this.__parentJS.loginRequest();
};

Login.prototype.onclickChangePassword = function (){
	this.__parentJS.expiredPasswordRequest();
};

Login.prototype.onclickCloseMessageButton = function (){
	this.__parentJS.hideMessage();
};

Login.prototype.onclickForgotPassword = function (){
	this.__parentJS.forgotPasswordRequest();

};
/**
 * when a key is pressed inside __divWrapContent we will treat it here.
 * @param  {Object} e - keyboard event object
 */
Login.prototype.onKeyPressAction = function (e){
	if (e.keyCode == 13){ // enter pressed
		this.__parentJS.onEnterPressAction();	
	}
	

};
