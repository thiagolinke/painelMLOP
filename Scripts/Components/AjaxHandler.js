/**
 * 
 * @param p_parent A reference to the object instantiating this one. Can be a slot or a component
 * @constructor
 * @class
 * @author Marcelo Negri Pimentel
 * @returns
 */
AjaxHandler = function(p_parent){
	this.__parent = p_parent;
};

AjaxHandler.prototype.doJsonAjaxRequest = function(p_url, p_formData, p_loadingOption) {
	this.__parent.showLoading(p_loadingOption);

	var jsonResponseHandler = function(jsonData) {
		this.contextObject.hideLoading(p_loadingOption);

		// this "d" property used as wrapper to avoid possible compatibility problems - investigate it more carefully
		// "data" is name that is used in JS code that will be executed
		var data = jsonData.d.data;
		var codeToExecute = jsonData.d.executableCodeArray;

		if (typeof codeToExecute != "undefined") {
			// An including the index variable in the code array is needed to prevent 
			// interference code inside Eval() operator from it modification during evaluation operations
			for(codeToExecute.indexVariable = 0; codeToExecute.indexVariable < codeToExecute.length; ++codeToExecute.indexVariable) {
				try{
					eval(codeToExecute[codeToExecute.indexVariable]);					
				}
				catch (err)
				{
					this.contextObject.printErrorMessage(err + "" + codeToExecute[codeToExecute.indexVariable]);
				}
			}
		}

	};


	var v_formData = new FormData();

	var v_url = "";

	if (p_formData) {
		if (typeof p_formData == "object" & p_formData.constructor.toString().indexOf("FormData") == -1) {
			v_formData = Utilities.buildFormDataBasedOnUrlParameters(p_formData);
			v_url = Utilities.buildStringUrlParameters(p_url, p_formData);
		} else {
			v_formData = p_formData;
			v_url = p_url + " (Currently isn't possible to retrieve the url parameters for the function " + this.__parent.constructor.name + ")";
		}
	}


	//we need to always append the active user to the data so if any errors happen we'll
	//know which user generated the error to give him a feedback
	//v_formData.append("u", TABS_INTERFACE.activeUser);
		
	var v_requestConfig = new Object();	
	var v_fileElementId;
	v_requestConfig.url = p_url;
	v_requestConfig.type = "POST";
	v_requestConfig.dataType = "json";
	v_requestConfig.success = jsonResponseHandler;
	if(navigator.appName == "Microsoft Internet Explorer"){
		v_requestConfig.data = v_formData.getParams();
		//André Martins: variables of jquery, different values IE / Mozilla
		v_requestConfig.processData = true;
		v_requestConfig.contentType = "application/x-www-form-urlencoded";
				
		//André Martins: check if there is a file type in the request data
		for (var i in v_requestConfig.data ){
			if((v_requestConfig.data[i].type != undefined ) && (v_requestConfig.data[i].type == "file")){
				v_fileElementId = i;
				v_requestConfig.contentType = "text/html";
			}
		}
		
		if(v_fileElementId){
			v_requestConfig.fileElementId = v_fileElementId;						
			v_requestConfig.contextObject = this.__parent;
			v_requestConfig.url = p_url;						
		}
		else{
			v_requestConfig.context = {contextObject: this.__parent, url : p_url};
		}
	}	      	  
	else{
		v_requestConfig.data = v_formData;
		//André Martins: variables of jquery, different values IE / Mozilla
		v_requestConfig.processData = false;
		v_requestConfig.contentType = false;
		v_requestConfig.context = {contextObject: this.__parent, url : p_url};
	}
	
	if (v_fileElementId) {	
		$.ajaxFileUpload(v_requestConfig);
	}else{
		 $.ajax(v_requestConfig);	
	}

};
