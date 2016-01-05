function Team()
{
	this.__ajaxHandler = new AjaxHandler(this);

	this.__elencoDiv = document.getElementById("meuElenco");

	//this.__mainContainer = generateMainContainer(this)
	
	//this.teamCollapsibleDiv = new CollapsibleDiv("Elenco", false);
	//this.teamCollapsibleDiv.setContent(this.generate_teamDiv());

	//this.gamesCollapsibleDiv = new CollapsibleDiv("Jogos", false);

	//this.htmlElement.appendChild(this.__loading_panel);
	//this.htmlElement.appendChild(this.teamCollapsibleDiv.htmlElement);
	//this.htmlElement.appendChild(this.gamesCollapsibleDiv.htmlElement);

	this.__getTeamContents();
}

Team.prototype.__getTeamContents = function ()
{
	var v_url = "team.ashx?f=get_contents";
	this.__ajaxHandler.doJsonAjaxRequest(v_url);
}

Team.prototype.getTeam = function ()
{
	var v_url = "team.ashx?f=get_team";
	this.__ajaxHandler.doJsonAjaxRequest(v_url);
}

Team.prototype.setPlayers = function (p_data)
{
	var v_div = ELEMENT_FACTORY.createElement("div", { className: "panel-body" });
	v_div.appendChild(this.generate_playerListTable(p_data));
	this.__elencoDiv.appendChild(v_div);

}

Team.prototype.generate_playerListTable = function (p_data)
{
	var v_table;
	var v_tr;
	var v_td;

	v_table = ELEMENT_FACTORY.createElement("table", { className: "table table-striped table-condensed" });

	var v_thead = ELEMENT_FACTORY.createElement("thead");
	v_tr = ELEMENT_FACTORY.createElement("tr");

	var v_th = ELEMENT_FACTORY.createElement("th", { textContent: "Posição" });
	v_tr.appendChild(v_th);

	v_th = ELEMENT_FACTORY.createElement("th", { textContent: "Nome" });
	v_tr.appendChild(v_th);

	v_th = ELEMENT_FACTORY.createElement("th", { textContent: "Idade" });
	v_tr.appendChild(v_th);

	v_th = ELEMENT_FACTORY.createElement("th", { textContent: "Salário" });
	v_tr.appendChild(v_th);

	v_th = ELEMENT_FACTORY.createElement("th", { textContent: "Multa" });
	v_tr.appendChild(v_th);

	v_thead.appendChild(v_tr);
	v_table.appendChild(v_thead);

	var v_tbody = ELEMENT_FACTORY.createElement("tbody");

	for (i = 0; i < p_data.length; i++)
	{
		v_tr = ELEMENT_FACTORY.createElement("tr");
		v_tr.__parentJS = this;
		v_tr.selectedID = p_data[i][0];
		v_tr.onclick = this.getPlayerStats;
		v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i][1] });
		v_tr.appendChild(v_td);
		v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i][2] });
		v_tr.appendChild(v_td);
		v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i][3] });
		v_tr.appendChild(v_td);
		v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i][4] });
		v_tr.appendChild(v_td);
		v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i][5] });
		v_tr.appendChild(v_td);
		v_tbody.appendChild(v_tr);
	}

	v_table.appendChild(v_tbody);

	return v_table;
}

Team.prototype.printErrorMessage = function (p_data)
{
	alert(p_data);
}