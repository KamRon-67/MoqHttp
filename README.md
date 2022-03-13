# MoqHttp

<div id="top"></div>
<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/KamRon-67/MoqHttp">
    <img src="https://github.com/KamRon-67/MoqHttp/blob/KamRon-67-patch-1/lines.png" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">MoqHttp</h3>

  <p align="center">
    An awesome Moq Rest Client project to jumpstart your projects!
    <br />
    <br />
    ·
    <a href="https://github.com/KamRon-67/MoqHttp/issues">Report Bug</a>
    ·
    <a href="https://github.com/KamRon-67/MoqHttp/issues">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

This is a lightweight mocking tool for .net 6 plus. The goal is to stay light and easy to integrate into your project.

Here's why:
* Your time should be focused on creating something amazing. A project that solves a problem and helps others
* You shouldn't be doing the same tasks over and over like creating a service project to mock your HTTP requests. 
* You should implement DRY principles to the rest of your life :smile:

Of course,  your needs may be different. So I'll be adding more features shortly. You may also suggest changes by forking this repo and creating a pull request or opening an issue. 


<p align="right">(<a href="#top">back to top</a>)</p>



### Built With

This section should list any major frameworks/libraries used to bootstrap your project. Leave any add-ons/plugins for the acknowledgements section. Here are a few examples.

* [Json.NET](https://www.newtonsoft.com/json)
* [Json.NET Schema](https://www.newtonsoft.com/jsonschema)
* [.Net 6](https://docs.microsoft.com/en-us/aspnet/core/?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

This is an example of how to list things you need to use the software and how to install them.
* nuget
  ```sh
  Install-Package MoqHttp -Version 1.0.0
  ```

### Installation

You should beable to download and run out of the box after a build. 



<p align="right">(<a href="#top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

```csharp
    [Fact]
        public async void Get_Should_Work_Correctly()
        {
            //Arrange
            _mockServer = new HttpServer(Port);
            _mockServer.Run();

            //Act
            _mockServer.Config.Get("/test/123").Send("It Really Works!");
            var responseGet = await _httpClient.GetAsync($"{_address}/test/123");

            _mockServer.Config.Get("/testAction/123").Send(context =>
            {
                context.Response.StatusCode = 200;
                const string response = "Action Test";
                var buffer = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            });
            var responseGetAction = await _httpClient.GetAsync($"{_address}/testAction/123");
            _mockServer.Dispose();

            //Assert
            Assert.Equal("It Really Works!", await responseGet.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)responseGet.StatusCode);

            Assert.Equal("Action Test", await responseGetAction.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)responseGetAction.StatusCode);
        }
  ```

More comming soon

<!-- ROADMAP -->
## Roadmap

- [x] Add Changelog
- [x] Add back to top links
- [ ] Clean up docs
- [ ] Add Additional Templates w/ Examples
- [ ] Add "components" document to easily copy & paste sections of the readme
- [ ] Return xml and json
    - [ ] Json
    - [ ] xml

See the [open issues](https://github.com/KamRon-67/MoqHttp/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Cameron Young - [@sigfualt](https://twitter.com/@sigfualt) - cameronyoung@cmmsolutions.io

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

Use this space to list resources you find helpful and would like to give credit to. I've included a few of my favorites to kick things off!

* [Choose an Open Source License](https://choosealicense.com)
* [GitHub Emoji Cheat Sheet](https://www.webpagefx.com/tools/emoji-cheat-sheet)
* [Img Shields](https://shields.io)
* [GitHub Pages](https://pages.github.com)
* [Font Awesome](https://fontawesome.com)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/KamRon-67/MoqHttp.svg?style=for-the-badge
[contributors-url]: https://github.com/KamRon-67/MoqHttp/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/KamRon-67/MoqHttp.svg?style=for-the-badge
[forks-url]: https://img.shields.io/github/forks/KamRon-67/MoqHttp
[stars-shield]: https://img.shields.io/github/stars/KamRon-67/MoqHttp.svg?style=for-the-badge
[stars-url]: https://github.com/othneildrew/Best-README-Template/stargazers
[issues-shield]: https://img.shields.io/github/issues/KamRon-67/MoqHttp.svg?style=for-the-badge
[issues-url]: https://github.com/KamRon-67/MoqHttp/issues
[license-shield]: https://img.shields.io/github/license/KamRon-67/MoqHttp.svg?style=for-the-badge
[license-url]: https://github.com/KamRon-67/MoqHttp/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/othneildrew
[product-screenshot]: images/screenshot.png
