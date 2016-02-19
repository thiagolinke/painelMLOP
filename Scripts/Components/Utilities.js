/**
 * 
 * @returns {Utilities}
 */
/**
 * Global var used as an attachment to the panel's IDs, to generate unique IDs. 
 * */
var g_iteratorId = 0;

function Utilities(){
	/* Andre Teixeira 2011-09-20: this place holder variable is used to make eclipse mount the JSdoc for this class normally */	
	this.__placeHolder = "";
}

/**
 * 
 * @param {String} p_array1 @type Array
 * @param {String} p_array2 
 * @param {Boolean} p_caseSensitive OPTIONAL=false true to take the string case in consideration
 * @returns {Array}
 */
Utilities.ArrayIntersection = function(p_array1,p_array2,p_caseSensitive) {
	var v_result = new Array();
	for(var i=0;i<p_array1.length;i++)
	{
		for(var j=0;j<p_array2.length;j++)
		{
			if(typeof(p_array1[i])=="string" && typeof(p_array2[j]) == "string")
			{
				if(p_caseSensitive)
				{
					if(p_array1[i] == p_array2[j])
					{
						v_result.push(p_array1[i]);
						break;
					}
				}
				else
				{
					if(p_array1[i].toLowerCase() == p_array2[j].toLowerCase())
					{
						v_result.push(p_array1[i]);
						break;
					}

				}
			}
			else
			{
				if(p_array1[i] == p_array2[j])
				{
					v_result.push(p_array1[i]);
					break;
				}
			}
		}
	}
	return v_result;
};

Utilities.BrowserIf = function(p_valueIE,p_valueOther){
	if(navigator.appName == 'Microsoft Internet Explorer')
		v_variable = p_valueIE;
	else
		v_variable = p_valueOther;

	return v_variable;

};

Utilities.RemoveLastChar = function (p_string, p_amount){

	if(!p_amount){
		p_amount = 1;
	}

	if (p_string.Length < p_amount) {
		p_amount = p_string.length;
	}

	p_string = p_string.substring(0, p_string.length - p_amount);

	return p_string;
};

Utilities.RandomString = function() {
	var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXTZabcdefghiklmnopqrstuvwxyz";
	var string_length = 8;
	var randomstring = '';
	for (var i=0; i<string_length; i++) {
		var rnum = Math.floor(Math.random() * chars.length);
		randomstring += chars.substring(rnum,rnum+1);
	}
	return randomstring;
};

Utilities.DateFromString = function(p_stringDate){
	var v_dateAr = p_stringDate.split("-");
	var y,m,d;

	var v_date = new Date();
	if (v_dateAr.length==3)
	{
		y=parseInt(v_dateAr[0],10);
		m=parseInt(v_dateAr[1],10)-1;
		d=parseInt(v_dateAr[2],10);
		v_date.setUTCFullYear(y);
		v_date.setUTCMonth(m);
		v_date.setUTCDate(d);

		//Hour is defined as midnight because some functions do operations with dates
		v_date.setHours(0); v_date.setMinutes(0); v_date.setSeconds(0); v_date.setMilliseconds(0);
	}
	return v_date;
};


/*
 * Returns an String Date formated as '2014-03-18 12:25:17', time is optional
 * 
 * Modified by Fidelis on 2014-03-19
 * 
 * @param p_date {Object} Date Object used to create a String
 * @param p_showTime {boolean} Optional value used to show time in String by default p_showTime = false
 * @return v_date {String} A string from date object in format '2014-03-18 12:25:17'
 * 
 */
Utilities.DateToString = function(p_date, p_showTime){

	//parameters validations -----------------------------------------
	if (p_showTime == undefined){
		p_showTime = false;
	}

	if(p_date instanceof Date){}
	else{
		p_date = new Date();
	}

	//----------------------------------------------------------------

	var v_month = p_date.getMonth() + 1;
	var v_day = p_date.getDate();
	var v_dateString = p_date.getFullYear();

	v_dateString += "-" + ( v_month < 10 ? "0" + v_month : v_month);
	v_dateString += "-" + (v_day < 10 ? "0" + v_day : v_day);	

	//set time on string if it is required-----------------------------
	if(p_showTime){
		var v_hours = p_date.getHours()+"";
		v_hours = (v_hours.length > 1) ? v_hours : "0" + v_hours;

		var v_minutes = p_date.getMinutes()+"";
		v_minutes = (v_minutes.length > 1) ? v_minutes : "0" + v_minutes;

		var v_seconds = p_date.getSeconds()+"";
		v_seconds = (v_seconds.length > 1) ? v_seconds : "0" + v_seconds;		

		v_dateString += " " + v_hours + ":" + v_minutes + ":" + v_seconds;
	}
	//-----------------------------------------------------------------
	return v_dateString;
};

/** Converts numeric degrees to radians */
Number.prototype.toRad = function() {
	return this * Math.PI / 180;
};

/** Converts radians to numeric (signed) degrees */
Number.prototype.toDeg = function() {
	return this * 180 / Math.PI;
};

String.prototype.startsWith = function(str) {
	return (this.match("^"+str) == str);
};

String.prototype.endsWith = function(str) {
	return (this.match(str+"$") == str);
};

/**
 * Capitalize the first letter of the string and makes the rest of the string lower case
 * @param {String} p_string The string to be capitalized
 * @returns The Capitalized String
 */
Utilities.Capitalize = function(p_string, p_lowerString){
	return p_string.charAt(0).toUpperCase() + (p_lowerString ? p_string.slice(1).toLowerCase() : p_string.slice(1));
};

/**
 * Remove all occurrences of a given value from an array
 * @param p_array The array to be searched
 * @param p_value The value to be removed
 * @returns The array without the values equal p_value
 */
Utilities.RemoveValueFromArray = function(p_array, p_value){
	var v_array = $.grep(p_array, function(value){
		return value != p_value;
	});
	return v_array;
};

/**
 * 
 * @param {Matrix} p_data A matrix of HTML elements with the data to load in each cell of the table
 * @returns {TableBodyHTMLElement}
 */
Utilities.ContructHtmlTableBody = function(p_data){
	var v_tableBody = ELEMENT_FACTORY.createElement("tbody");
	var v_tr, v_td;

	for (var i=0; i < p_data.length; i++){
		v_tr = ELEMENT_FACTORY.createElement("tr");
		if (p_data[i] != undefined) {
			for (var j=0; j < p_data[i].length; j++){
				v_td = ELEMENT_FACTORY.createElement("td");
				v_td.appendChild(p_data[i][j]);
				v_tr.appendChild(v_td);
			}

		}
		v_tableBody.appendChild(v_tr);
	}

	return v_tableBody;
};

/**
 * This function is used to build the FormData object used on fileAjax request based on a string with
 * these parameters
 * @param p_urlParameters {String|Array} The url Parameters after the ? character, e.g gridviewType=asd&name=123&type=543
 * or a matrix with the url parameters, e.g [["gridviewType","asd"], ["name", "123"], ["type", "543"]]
 * @returns {FormData}
 * 
 * Created by Andre Teixeira on 2012-02-03
 * Modified by Andre Teixeira on 2012-04-20 to accept also a matrix of parameters
 * e.g: [['id', 'nokia3g__rnc__3'],
 *      ['dateFrom', '2012-04-20'],
 *      ['dateTo', '2012-04-20']]
 *      
 * Modified by Lucas Gustavo on 2012-09-10 to accept also a object of parameters
 */
Utilities.buildFormDataBasedOnUrlParameters = function(p_urlParameters){
	var v_isArrayParameters;
	if(typeof p_urlParameters == "string"){
		v_isArrayParameters = false;
	}else{
		v_isArrayParameters = true;
	}

	var v_formData = new FormData();
	var v_url = v_isArrayParameters ? p_urlParameters : p_urlParameters.split("&");

	for(var i in v_url){
		var v_splitedVal = v_isArrayParameters ? v_url[i] : v_url[i].split("=");

		var v_content = "";
		var v_parameterName = "";

		if(! isNaN(parseInt(i))){
			for(var j = 1; j < v_splitedVal.length; j++){
				if(j > 1) v_content += "=";
				v_content += v_splitedVal[j];
			}
			v_parameterName = v_splitedVal[0];
		}else{
			v_parameterName = i;
			v_content = v_url[i];
		}

		//André Martins on 2014-06-13: Remove char strange
		if(typeof(v_content) == "string" && (v_content != undefined)){
			v_content = v_content.replace(/\u00A0/g,"");
		}		
		
		v_formData.append(v_parameterName, v_content);
	}

	return v_formData;
};

Utilities.buildObjectBasedOnUrlParameters = function(p_urlParameters){
	var v_object = new Object();
	var v_url = p_urlParameters.split("&");

	for(var i in v_url){
		var v_splitedVal = v_url[i].split("=");

		var v_content = "";
		var v_parameterName = "";

		if(! isNaN(parseInt(i))){
			for(var j = 1; j < v_splitedVal.length; j++){
				if(j > 1) v_content += "=";
				v_content += v_splitedVal[j];
			}
			v_parameterName = v_splitedVal[0];
		}else{
			v_parameterName = i;
			v_content = v_url[i];
		}
		v_object[v_parameterName]= v_content;
	}

	return v_object;
};

Utilities.buildStringUrlParameters = function (p_url, p_urlParameters){
	var v_finalUrl = "";

	v_finalUrl = p_url + "?";

	for(var parameter in p_urlParameters){
		v_finalUrl += parameter + "=" + p_urlParameters[parameter] + "&";
	}

	v_finalUrl = Utilities.RemoveLastChar(v_finalUrl);

	return v_finalUrl;

};


/**
 * Generating an iterating ID
 * @param p_id {String} the prefix for the Id
 * @returns v_idGen {String}
 * */
Utilities.generateId = function(p_id){
	var v_idGen = "";

	v_idGen = p_id.replace(/ /g, "");
	v_idGen += g_iteratorId;

	g_iteratorId++;

	return v_idGen;
};

Utilities.isEmptyObject = function(p_obj){
	for(undefined in p_obj){
		return false;
	}

	return true;
};

Utilities.stopPropagation = function (p_event){
	if(!p_event){
		p_event = window.event;
	}
	p_event.cancelBubble = true;
	if(p_event.stopPropagation) p_event.stopPropagation();
};

Utilities.constructorName = function(p_object){
	var v_name;
	
	//Barbara Perdigao on 2013-10-22: the property "constuctor.name" doesn't work for Internet Explorer,
	//so I added this condition to get the function name in a different way for IE. 
	if (navigator.appName == "Microsoft Internet Explorer") {
		var v_result = p_object.constructor.toString().match(/function (.{1,})\(/);
		if (v_result != null && v_result.length > 0) {
			v_name = v_result[1];
		}
	} else {
		v_name = p_object.__proto__.constructor.name;
	}
	return v_name;
};

/**
 * 
 * @param p_hex
 * @param p_alpha The alpha parameter is a number between 0.0 (fully transparent) and 1.0 (fully opaque).
 * @returns {String}
 */
Utilities.hex2rgba = function(p_hex, p_opacity){
    //extract the two hexadecimal digits for each color
    var v_patt = /^#([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})$/;
    var v_matches = v_patt.exec(p_hex);
    var v_opacityPart;
    var v_function;
    
    if(p_opacity == undefined){
    	v_opacityPart = "";
    	v_function = "rgb";
    }
    else{
    	v_opacityPart = "," + p_opacity;
    	v_function = "rgba";
    }

    //convert them to decimal
    var r = parseInt(v_matches[1], 16);
    var g = parseInt(v_matches[2], 16);
    var b = parseInt(v_matches[3], 16);
    
    //create rgba string
    var rgba = v_function + "(" + r + "," + g + "," + b + v_opacityPart + ")";

    //return rgba colour
    return rgba;
};

//Additional functions (these function were moved from main_logic and can't be inside Utilities class because there are a lot of code using them already)
function isNumber(p_str) {
	return isInteger(p_str);
}

function isNumeric(n) {
	return !isNaN(parseFloat(n)) && isFinite(n);
}

function isInteger(s) {
	var i;

	if (isEmpty(s)){
		if (isInteger.arguments.length == 1) return 0;
		else return (isInteger.arguments[1] == true);
	}
	for (i = 0; i < s.length; i++) {
		var c = s.charAt(i);

		if (!isDigit(c) && (i==0 && c!='-')) return false;
	}

	return true;
}

function isEmpty(s) {
	return ((s == null) || (s.length == 0));
}

function isDigit(c) {
	return ((c >= "0") && (c <= "9"));
}

function trim(str, chars) {
	return ltrim(rtrim(str, chars), chars);
}

function ltrim(str, chars) {
	chars = chars || "\\s";
	return str.replace(new RegExp("^[" + chars + "]+", "g"), "");
}

function rtrim(str, chars) {
	chars = chars || "\\s";
	return str.replace(new RegExp("[" + chars + "]+$", "g"), "");
}

/**
 * Left-aligns the characters in this string, padding on the right with a specified Unicode character, for a specified total length.
 * 
 * @param str The string to be padded
 * @param totalWidth The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.
 * @param paddingChar A Unicode padding character.
 * @returns A new String that is equivalent to this instance, but left-aligned and padded on the right with as many paddingChar characters 
 * as needed to create a length of totalWidth. Or, if totalWidth is less than the length of this instance, a new System.String that is identical to this instance.
 * 
 * Created by Andre Teixeira on 2014-04-11: This function has the exact same behaviour of the .NET function  PadRight
 */
function PadRight(str, len, pad) {
	if (typeof (pad) == "undefined") { var pad = ' '; }
	if (len + 1 >= str.length) {
		str = str + Array(len + 1 - str.length).join(pad);
	}
	return str;
}

/**
 * Left-aligns the characters in this string, padding on the right with a specified Unicode character, for a specified total length.
 * 
 * @param str The string to be padded
 * @param totalWidth The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.
 * @param paddingChar A Unicode padding character.
 * @returns A new String that is equivalent to this instance, but left-aligned and padded on the right with as many paddingChar characters 
 * as needed to create a length of totalWidth. Or, if totalWidth is less than the length of this instance, a new System.String that is identical to this instance.
 * 
 * Created by Andre Teixeira on 2014-04-11: This function has the exact same behaviour of the funtion common_functions.EncriptString
 */
function encriptPassword(p_key, p_toEncript) {
	var v_encriptationSequence = [1, 3, 1, 9, 3, 2, 8, 2, 2, 3];
	var v_paddingSequence = ["A", "d", "X", "E", "Y", "E", "b", "a", "l", "o"];

	var v_paddingSequenceSize = v_paddingSequence.length;
	var v_encriptationSequenceSize = v_encriptationSequence.length;
	var v_keyLength = p_key.length;

	var v_wordSize = 5;
	var v_toReturn = "";

	for (var i = 0; i < p_toEncript.length; i++) {
		var v_tempString = p_toEncript.charCodeAt(i) + (v_encriptationSequence[i % v_encriptationSequenceSize] * p_key.charCodeAt(i % v_keyLength));
		v_tempString = PadRight(v_tempString + '', v_wordSize, v_paddingSequence[i % v_paddingSequenceSize]);
		v_toReturn += '' + v_tempString;
	}

	return v_toReturn;
}
