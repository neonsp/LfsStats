
function ButtonClick(button)
{
	buttons = document.getElementById('mainMenu').getElementsByTagName("a");
	
	for(var i = 0; i < buttons.length; i++) // deactivate all buttons
	{
			buttons[i].className = "";
	}
	button.className = "active"; // activate clicked button
}

function DisableAllpages()
{
	document.getElementById('page1').style.display = 'none';
	document.getElementById('page2').style.display = 'none';
	document.getElementById('page3').style.display = 'none';
	document.getElementById('page4').style.display = 'none';
	document.getElementById('page5').style.display = 'none';
	document.getElementById('page6').style.display = 'none';
	document.getElementById('page7').style.display = 'none';
	document.getElementById('page8').style.display = 'none';
	document.getElementById('page9').style.display = 'none';
}

function Button1OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page1').style.display = 'block';
}

function Button2OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page2').style.display = 'block';
}

function Button3OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page3').style.display = 'block';
}

function Button4OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page4').style.display = 'block';
}

function Button5OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page5').style.display = 'block';
}

function Button6OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page6').style.display = 'block';
}

function Button7OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page7').style.display = 'block';
}

function Button8OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page8').style.display = 'block';
}

function Button9OnClick(button)
{
	ButtonClick(button);
    
	DisableAllpages();
	document.getElementById('page9').style.display = 'block';
}