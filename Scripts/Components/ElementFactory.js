/**
 * Utility Class - used to create DOM elements according to applicaiton architecture.
 * 
 * ELEMENT_FACTORY.createElement("<DOM element name>", {<tag attributes map>}, {<element styles map>}, <validation_type>);
 *
 * Examples of use:
 * this.__saveButton = ELEMENT_FACTORY.createElement("input",
 *              {type:"button",value:"Save",__parentJS:this,onclick:this.__onClickSave},
 *              {position:"absolute",left:"800px",top:"7px"});
 * 
 * v_spanElement = ELEMENT_FACTORY.createElement("span", 
 * 						{className: "main_text_box", textContent: "some text"}, 
 * 						{fontSize: "11px"});
 * 
 * this.__weeksInput = ELEMENT_FACTORY.createElement("input", 
 * 							{type: "text", className: "main_text_box", value: "1"}, 
 * 							{width: "20px"},"greaterthanzero");
 * 
 * this.__agregationType = ELEMENT_FACTORY.createElement('select', {className: 'main_text_box'});
 * v_option1 = ELEMENT_FACTORY.createElement('option', {value:'1',textContent:'Menu 1'});
 * this.__agregationType.appendChild(v_option1);
 * 
 * var v_tr = ELEMENT_FACTORY.createElement("tr");
 * v_table.appendChild(v_tr);
 */
var ELEMENT_FACTORY = {
		
		/**
		 * 
		 * @param p_elementName {String}
		 * @param p_options {Object}
		 * @param p_styles {CSSStyleDeclaration}
		 * @param p_inputValidation {String}
		 * @returns {HTMLElement}
		 */
		createElement: function(p_elementName,p_options,p_styles,p_inputValidation,p_selectOptions)
		{
			var v_element = document.createElement(p_elementName);
			if(p_options)
			{
				jQuery.extend(v_element,p_options);	
			}
			if(p_styles)
			{
				jQuery.extend(v_element.style,p_styles);
			}
			switch(p_elementName.toLowerCase()){
				case "input":
					if(p_inputValidation)
					{
						v_element.validate = ELEMENT_FACTORY.generateValidation(p_inputValidation);
					}					
					break;
				case "select":
					if(p_selectOptions){
						for(var t_selectOption in p_selectOptions)
						{
							var v_option = document.createElement("option");
							v_option.value = p_selectOptions[t_selectOption];
							v_option.name = t_selectOption.toLowerCase();
							v_option.textContent = t_selectOption;
							v_element.appendChild(v_option);
						}					
					}									
					break;
			}	
			return v_element;
		},
		generateValidation: function(p_type)
		{
			
			switch(p_type.toLowerCase())
			{
			
			case "email":
				return function(){
					if(this.value == "")
					{
						return false;
					}
					if(this.value.indexOf("@") == -1)
					{
						return false;
					}
					if(this.value.split("@")[0].length == 0)
					{
						return false;
					}
					if(this.value.split("@")[1].length == 0)
					{
						return false;
					}
					return true;
				};
			case "number":
				return function()
				{
					if(this.value == "")
					{
						return false;
					}
					if(!isInteger(this.value))
					{
						return false;
					}
					return true;
				};
			case "positivenumber":
				return function()
				{
					if(this.value == "")
					{
						return false;
					}
					if(!isInteger(this.value))
					{
						return false;
					}
					if(this.value < 0)
					{
						return false;
					}
					return true;
				};
			/*
			  Andre Teixeira:
			  "positivenumber" returns true to values equal 0, since 0 IS a positive number.
			  In the other hand "greaterthanzero" returns false if the value is equal 0.
			  can you see the difference in the logic?
			  
			  --- Note that this function will accept only integers. ---
			*/
			case "greaterthanzero":
				return function()
				{
					if(this.value == "")
					{
						return false;
					}
					if(!isInteger(this.value))
					{
						return false;
					}
					if(this.value < 1)
					{
						return false;
					}
					return true;
				};
			}
			
		}

};