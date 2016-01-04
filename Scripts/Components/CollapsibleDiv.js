/**
 * 
 * @param p_header {String}||{HTMLAnchorElement} The text content of the header, can also be an HTMLAnchorElement
 * @param p_hiddenOnCreate {Boolean} IF this div should be hidden on not in the first time
 * @returns {CollapsibleDiv}
 */
function CollapsibleDiv(p_header,p_hiddenOnCreate){

	if(!p_hiddenOnCreate){
		p_hiddenOnCreate = false;
	}
	
	this.htmlElement = ELEMENT_FACTORY.createElement("div", { className: "ui-widget" });
	
	this.__wasClickedBefore = false;
	
	this.__headerElement = ELEMENT_FACTORY.createElement("h3", { className: "ui-widget-header" }, { padding: "0px" });
	this.__headerElement.__parentJS = this;
	this.__headerElement.onclick = this.__onClickHeader;
	
	if(typeof p_header == "string"){
		this.__headerTitle = ELEMENT_FACTORY.createElement("span", { textContent: p_header }, { fontSize: "12px", paddingLeft: "30px" });
	}
	else{
		this.__headerTitle = p_header;
	}	
	
	this.__headerElement.appendChild(this.__headerTitle);
	
	this.__bodyElement = ELEMENT_FACTORY.createElement("div", { className: "ui-widget-content" });

	this.htmlElement.appendChild(this.__headerElement);
	this.htmlElement.appendChild(this.__bodyElement);
	
	$(this.htmlElement).accordion({collapsible: true, active: !p_hiddenOnCreate, animated: false});
	
	$(this.htmlElement).bind("accordionchange", function(event, ui) {
		try{
			ui.newHeader[0].__parentJS.resizeContent();
		}catch(e)
		{
			//no catch needed
		}
	});
	
}

/**
 * Only works if the header was defined as a string on this object instantiation
 * @param p_header {String}
 */
CollapsibleDiv.prototype.setHeaderTitle = function(p_header){
	this.__headerTitle.textContent = p_header;		
};

/**
 * Only works if the header was defined as a string on this object instantiation
 * @param p_help {String}
 */
CollapsibleDiv.prototype.setHeaderHelp = function(p_help){
	this.__headerTitle.title = p_help;
};

CollapsibleDiv.prototype.collapse = function()
{
	if($(this.htmlElement).accordion("option","active") === false)
	{
		//return
	}
	else
	{
		$(this.htmlElement).accordion("activate",false);
	}
};

CollapsibleDiv.prototype.expand = function()
{
	if($(this.htmlElement).accordion("option","active") === 0)
	{
		//return
	}
	else
	{
		$(this.htmlElement).accordion("activate",0);
	}
};

CollapsibleDiv.prototype.applyBodyStyle = function(p_style)
{
	jQuery.extend(this.__bodyElement.style,p_style);
};


CollapsibleDiv.prototype.applyHeaderStyle = function(p_style)
{
	jQuery.extend(this.__headerElement.style,p_style);
};
CollapsibleDiv.prototype.applyHeaderTextStyle = function(p_style)
{
	jQuery.extend(this.__headerElement.children[1].style,p_style);
};
CollapsibleDiv.prototype.removePaddingAndResize = function()
{
	this.__wasClickedBefore = true;
	this.resizeContent();
};
CollapsibleDiv.prototype.resizeContent = function()
{
	$(this.htmlElement).accordion("refresh");
};

CollapsibleDiv.prototype.setContent = function(p_element)
{
	this.__bodyElement.appendChild(p_element);
	this.resizeContent();
};

CollapsibleDiv.prototype.clearContent = function()
{
	this.__bodyElement.innerHTML = "";
	this.resizeContent();
};
CollapsibleDiv.prototype.__onClickHeader = function()
{
	if(!this.__parentJS.__wasClickedBefore)
	{
		this.__parentJS.removePaddingAndResize();
	}
};