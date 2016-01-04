<%@ Page Language="VB" EnableSessionState="true" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta name="viewport" content="width=device-width, initial-scale=1">

  <title>Painel de Controle MLOP</title>

	<!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
	<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js"></script>

	<!-- Latest compiled and minified CSS -->
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css" integrity="sha384-1q8mTJOASx8j1Au+a5WDVnPi2lkFfwwEAa8hDDdjZlpLegxhjVME1fgjWPGmkzs7" crossorigin="anonymous">

	<!-- Optional theme -->
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap-theme.min.css" integrity="sha384-fLW2N01lMqjakBkx3l/M9EahuwpSfeNvV63J5ezn3uZzapT0u7EYsXMjQV+0En5r" crossorigin="anonymous">

	<!-- Latest compiled and minified JavaScript -->
	<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js" integrity="sha384-0mSbJDEHialfmuBBQP6A4Qrprq5OVfW37PRR3j5ELqxss1yVqOtnepnHVP9aJ7xS" crossorigin="anonymous"></script>

	<!-- Components -->
	<script type="text/javascript" src="Scripts/Components/Utilities.js?avoidCache=may26"></script>
	<script type="text/javascript" src="Scripts/Components/AjaxHandler.js?avoidCache=may26"></script>
	<script type="text/javascript" src="Scripts/Components/ElementFactory.js?avoidCache=may26"></script>

	<!-- Default -->
	<script type="text/javascript" src="Scripts/default.js?avoidCache=may26"></script>

</head>
<body style="padding-top: 70px;">

	<!-- Header 
	<div class="ui-widget-header">
    <table style="width:100%;">
			<tr>
				<td style="width: 25%;"></td>
				<td style="width: 25%;">Time</td>
				<td style="width: 25%;">Escudo</td>
				<td style="width: 25%;">Patrocínio</td>
			</tr>
    </table>
	</div>

	 -->


	<!-- Navbar -->
	<nav class="navbar navbar-default navbar-fixed-top">
  <div class="container">
    <!-- Brand and toggle get grouped for better mobile display -->
    <div class="navbar-header">
      <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
        <span class="sr-only">Toggle navigation</span>
        <span class="icon-bar"></span>
        <span class="icon-bar"></span>
        <span class="icon-bar"></span>
      </button>
      <a class="navbar-brand" href="#">Painel MLOP</a>
    </div>

    <!-- Collect the nav links, forms, and other content for toggling -->
    <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
      <ul class="nav navbar-nav">
			<li role="presentation"><a id="a_team" data-toggle="tab" href="#div_team">Meu Time</a></li>
			<li role="presentation" class="dropdown">
				<a class="dropdown-toggle" data-toggle="dropdown" href="#" role="button" aria-haspopup="true" aria-expanded="false"> Competições <span class="caret"></span></a>
				<ul class="dropdown-menu">
					<li><a data-toggle="tab" href="#div_mlop">MLOP</a></li>
					<li><a href="team.aspx">Yahoo</a></li>
					<li><a href="#">CDP</a></li>
					<li><a href="#">Sula</a></li>
				</ul>
			</li>
			<li><a href="#div_mercado">Mercado</a></li>
			<li><a href="#div_painel">Painel</a></li>
      </ul>

      <ul class="nav navbar-nav navbar-right">
        <p class="navbar-text"></p>
				<p class="navbar-text">PSG(thiagolinke)</p>
      </ul>
    </div><!-- /.navbar-collapse -->
  </div><!-- /.container-fluid -->
</nav>

	<div class="container" id="alerts_container"></div>

	<!-- Tabs Contents -->
	<div class="container tab-content">
		<div id="div_team" class="tab-pane" role="tabpanel"></div>
		<div id="div1" class="tab-pane" role="tabpanel"></div>
		<div id="div_mercado" class="tab-pane" role="tabpanel"></div>
		<div id="div_painel" class="tab-pane" role="tabpanel"></div>
	</div>

	<!-- Footnote -->
	<div class="footnote_text" style="border: solid #F0F0F0 1px; height: 30px; text-align: center; vertical-align: middle; padding-bottom: 5px; padding-top: 5px;">
	</div>


</body>
</html>
