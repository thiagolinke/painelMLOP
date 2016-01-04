/**
 * Everything in this function will be executed after the page loads
 */
$(document).ready(function ()
{
	$('.dropdown-toggle').dropdown();

	$("#a_team").click(function ()
	{
		$("#div_team").load("/team.html");
	});

});