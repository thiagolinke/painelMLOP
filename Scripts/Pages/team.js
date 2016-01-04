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

	this.__getTeamInfo();
}

Team.prototype.__getTeamInfo = function ()
{
	var v_url = "team.ashx?f=get_contents";
	this.__ajaxHandler.doJsonAjaxRequest(v_url);
}

Team.prototype.generate_teamDiv = function ()
{
	var v_tr;
	var v_td;

	var v_container = ELEMENT_FACTORY.createElement("table", {}, { width: "100%",  fontSize: "small" });
	v_tr = ELEMENT_FACTORY.createElement("tr");
	this.divPlayerList = ELEMENT_FACTORY.createElement("td", {}, { width: "30%" }, { verticalAlign: "top" });
	v_tr.appendChild(this.divPlayerList);
	this.divPlayerStats = ELEMENT_FACTORY.createElement("td", {}, { width: "70%" }, { verticalAlign: "top" });
	v_tr.appendChild(this.divPlayerStats);
	v_container.appendChild(v_tr);

	return v_container;
}

Team.prototype.setPlayers = function (p_data)
{
	var v_div = ELEMENT_FACTORY.createElement("div", { className: "panel-body" });
	v_div.appendChild(this.generate_playerListDiv(p_data));
	this.__elencoDiv.appendChild(v_div);

}

Team.prototype.generate_playerListDiv = function (p_data)
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

Team.prototype.getPlayerStats = function (sender)
{
	var v_url = "team.ashx?f=get_player_stats";
	var v_formData = new Object();
	v_formData["playerID"] = this.selectedID;

	this.__parentJS.__ajaxHandler.doJsonAjaxRequest(v_url, v_formData, "divPlayerStats");
}

Team.prototype.setPlayerStats = function (p_ID, p_data)
{
	this.divPlayerStats.appendChild(this.generate_playerStatsDiv(p_ID, p_data));
	this.teamCollapsibleDiv.resizeContent();

}

Team.prototype.generate_playerStatsDiv = function (p_ID, p_data)
{
	var v_fields1 = ["Nome", "Time", "Nacionalidade", "Altura", "Peso", "Idade", "Pé", "Posição", "positions"];
	var v_fields2 = ["Instinto artilheiro", "Cabeçada", "Retenção de bola", "Poder de iludir", "Drible (veloc.)", "Última bola", "Visão", "Infiltração", "Ritmo de jogo", "Interceptações", "Força física", "Cobertura", "Força pelo alto", "Precisão de lançam.", "Veloc. e agilidade", "Falta", "Escanteio", "Pênaltis"];
	var v_fields2GK = ["Força física", "Cobertura", "Força pelo alto", "Precisão de lançam.", "Veloc. e agilidade", "Oportunista", "Habil. bola aérea", "Cara a cara", "Habilidade na linha", "Consistência", "Falta", "Escanteio", "Pênaltis"];
	var v_fields3 = ["Habil. ofensiva", "Controle de bola", "Drible", "Passe rasteiro", "Passe pelo alto", "Finalização", "Chute colocado", "Giro controlado", "Cabeçada", "Habil. defensiva", "Desarme", "Força do chute", "Velocidade"];
	var v_fields4 = ["Explosão", "Equilíbrio", "Impulsão", "Resistência", "Habil. como goleiro", "Defesas", "Condição física", "Resistência a lesão", "Pior pé (frequência)", "Pior pé (precisão)", "Pontuação geral"];
	var v_fields5 = ["Estilos de jogo", "Habilidades do jogador", "Estilos de jogo do COM"];

	var v_tableContainer = ELEMENT_FACTORY.createElement("table", { className: "player" }, { display: "table", clear: "both" });
	var v_trContainer = ELEMENT_FACTORY.createElement("tr");
	var v_tdContainer = ELEMENT_FACTORY.createElement("td");

	var v_table;
	var v_tr;
	var v_th;
	var v_td;
	
	var v_isGK = false;
	if ((p_data[7] == "GO") || (p_data[7] == "GK"))
	{
		v_isGK = true;
	}

	var v_index = 0;

	v_table = ELEMENT_FACTORY.createElement("table");
	for (i=0;i<v_fields1.length;i++)
	{
		v_tr = ELEMENT_FACTORY.createElement("tr");
		v_th = ELEMENT_FACTORY.createElement("th", { textContent: v_fields1[i] + ":" });
		v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i + v_index] });
		v_tr.appendChild(v_th);
		v_tr.appendChild(v_td);
		v_table.appendChild(v_tr);
	}
	v_tr = ELEMENT_FACTORY.createElement("tr");
	v_td = ELEMENT_FACTORY.createElement("td");
	$(v_td).attr("colspan", "2");
	v_td.appendChild(ELEMENT_FACTORY.createElement("img", {className: "player_image", src: "Images/players/player_" + p_ID + ".png"}));
	v_tr.appendChild(v_td);
	v_table.appendChild(v_tr);

	v_tdContainer.appendChild(v_table);
	v_trContainer.appendChild(v_tdContainer);

	var v_class;

	v_index += v_fields1.length;

	var v_fieldsToUse;
	if (v_isGK)
	{
		v_fieldsToUse = v_fields2GK;
	}
	else
	{
		v_fieldsToUse = v_fields2;
	}

	v_tdContainer = ELEMENT_FACTORY.createElement("td");
	v_table = ELEMENT_FACTORY.createElement("table");
	for (i = 0; i < v_fieldsToUse.length; i++)
	{
		if (p_data[i + v_index] < 70) { v_class = ""; }
		else if (p_data[i + v_index] < 80) { v_class = "c1"; }
		else if (p_data[i + v_index] < 90) { v_class = "c2"; }
		else if (p_data[i + v_index] < 95) { v_class = "c3"; }
		else { v_class = "c4"; }
		v_tr = ELEMENT_FACTORY.createElement("tr");
		v_th = ELEMENT_FACTORY.createElement("th", { textContent: v_fieldsToUse[i] + ":" });
		v_td = ELEMENT_FACTORY.createElement("td", { className: v_class, textContent: p_data[i + v_index] });
		v_tr.appendChild(v_th);
		v_tr.appendChild(v_td);
		v_table.appendChild(v_tr);
	}
	v_tdContainer.appendChild(v_table);
	v_trContainer.appendChild(v_tdContainer);

	v_index += v_fieldsToUse.length;

	v_tdContainer = ELEMENT_FACTORY.createElement("td");
	v_table = ELEMENT_FACTORY.createElement("table");
	for (i = 0; i < v_fields3.length; i++)
	{
		if (p_data[i + v_index] < 70) { v_class = ""; }
		else if (p_data[i + v_index] < 80) { v_class = "c1"; }
		else if (p_data[i + v_index] < 90) { v_class = "c2"; }
		else if (p_data[i + v_index] < 95) { v_class = "c3"; }
		else { v_class = "c4"; }
		v_tr = ELEMENT_FACTORY.createElement("tr");
		v_th = ELEMENT_FACTORY.createElement("th", { textContent: v_fields3[i] + ":" });
		v_td = ELEMENT_FACTORY.createElement("td", { className: v_class, textContent: p_data[i + v_index] });
		v_tr.appendChild(v_th);
		v_tr.appendChild(v_td);
		v_table.appendChild(v_tr);
	}
	v_tdContainer.appendChild(v_table);
	v_trContainer.appendChild(v_tdContainer);

	v_index += v_fields3.length;

	v_tdContainer = ELEMENT_FACTORY.createElement("td");
	v_table = ELEMENT_FACTORY.createElement("table");
	for (i = 0; i < v_fields4.length; i++)
	{
		if (p_data[i + v_index] < 70) { v_class = ""; }
		else if (p_data[i + v_index] < 80) { v_class = "c1"; }
		else if (p_data[i + v_index] < 90) { v_class = "c2"; }
		else if (p_data[i + v_index] < 95) { v_class = "c3"; }
		else { v_class = "c4"; }
		v_tr = ELEMENT_FACTORY.createElement("tr");
		v_th = ELEMENT_FACTORY.createElement("th", { textContent: v_fields4[i] + ":" });
		v_td = ELEMENT_FACTORY.createElement("td", { className: v_class, textContent: p_data[i + v_index] });
		v_tr.appendChild(v_th);
		v_tr.appendChild(v_td);
		v_table.appendChild(v_tr);
	}
	v_tdContainer.appendChild(v_table);
	v_trContainer.appendChild(v_tdContainer);

	v_index += v_fields4.length;

	v_tdContainer = ELEMENT_FACTORY.createElement("td");
	$(v_tdContainer).attr("rowspan", "2");
	v_table = ELEMENT_FACTORY.createElement("table", { className: "playing_styles" });
	for (i = 0; i < v_fields5.length; i++)
	{
		v_tr = ELEMENT_FACTORY.createElement("tr");
		v_th = ELEMENT_FACTORY.createElement("th", { textContent: v_fields5[i] });
		v_tr.appendChild(v_th);
		v_table.appendChild(v_tr);
		for (j = 0; j < p_data[i + v_index].split(",").length; j++)
		{
			v_tr = ELEMENT_FACTORY.createElement("tr");
			v_td = ELEMENT_FACTORY.createElement("td", { textContent: p_data[i + v_index].split(",")[j] });
			v_tr.appendChild(v_td);
			v_table.appendChild(v_tr);
		}

	}
	v_tdContainer.appendChild(v_table);
	v_trContainer.appendChild(v_tdContainer);

	v_tableContainer.appendChild(v_trContainer);

	$(v_tableContainer).show();

	return v_tableContainer;

}

Team.prototype.showLoading = function (p_option)
{

}

Team.prototype.hideLoading = function (p_option)
{

}

Team.prototype.printErrorMessage = function (p_data)
{
	alert(p_data);
}