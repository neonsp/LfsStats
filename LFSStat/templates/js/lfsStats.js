
function ButtonClick(button)
{
	buttons = document.getElementById('mainMenu').getElementsByTagName("a");
	
	for(var i = 0; i < buttons.length; i++) // deactivate all buttons
	{
			buttons[i].className = "";
	}
	button.className = "active"; // activate clicked button
}

function DisableAllSections()
{
	document.getElementById('section1').style.display = 'none';
	document.getElementById('section2').style.display = 'none';
	document.getElementById('section3').style.display = 'none';
	document.getElementById('section4').style.display = 'none';
	document.getElementById('section5').style.display = 'none';
	document.getElementById('section6').style.display = 'none';
	document.getElementById('section7').style.display = 'none';
	document.getElementById('section8').style.display = 'none';
}
    
function Button1OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section1').style.display = 'block';
}

function Button2OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section2').style.display = 'block';
}

function Button3OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section3').style.display = 'block';
}

function Button4OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section4').style.display = 'block';
}

function Button5OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section5').style.display = 'block';
}

function Button6OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section6').style.display = 'block';
}

function Button7OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section7').style.display = 'block';
}

function Button8OnClick(button)
{
	ButtonClick(button);
    
	DisableAllSections();
	document.getElementById('section8').style.display = 'block';
}