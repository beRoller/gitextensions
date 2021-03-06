﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RevisionFileTreeControllerTests
    {
        private IGitModule _module;
        private IFileAssociatedIconProvider _iconProvider;
        private RevisionFileTreeController _controller;
        private TreeNode _rootNode;
        private ImageList _imageList;


        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _iconProvider = Substitute.For<IFileAssociatedIconProvider>();
            _controller = new RevisionFileTreeController(_module, _iconProvider);

             _rootNode = new TreeNode();
             _imageList = new ImageList();
        }

        [TearDown]
        public void TearDown()
        {
            _imageList?.Dispose();
            _imageList = null;
        }


        [Test]
        public void LoadItemsInTreeView_should_add_all_none_GitItem_items_with_1st_level_nodes()
        {
            var items = new IGitItem[] { new MockGitItem("file1"), new MockGitItem("file2") };

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageIndex.Should().Be(-1);
                _rootNode.Nodes[i].SelectedImageIndex.Should().Be(-1);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(1);
            }
            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_IsTree_as_folders()
        {
            var items = new[] { CreateGitItem("file1", true, false, false), CreateGitItem("file2", true, false, false) };

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Folder);
                _rootNode.Nodes[i].SelectedImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Folder);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(1);
            }
            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_IsCommit_as_submodue()
        {
            var items = new[] { CreateGitItem("file1", false, true, false), CreateGitItem("file2", false, true, false) };

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be($"{items[i].Name} (Submodule)");
                _rootNode.Nodes[i].ImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Submodule);
                _rootNode.Nodes[i].SelectedImageIndex.Should().Be(RevisionFileTreeController.TreeNodeImages.Submodule);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
            }
            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_IsBlob_as_file()
        {
            var items = new[] { CreateGitItem("file1", false, false, true), CreateGitItem("file2", false, false, true) };

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
            }
        }

        [Test]
        public void LoadItemsInTreeView_should_not_load_icons_for_file_without_extension()
        {
            var items = new[] { CreateGitItem("file1.", false, false, true), CreateGitItem("file2", false, false, true) };

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].SelectedImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
            }
            _imageList.Images.Count.Should().Be(0);
            _iconProvider.DidNotReceive().Get(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void LoadItemsInTreeView_should_not_add_icons_for_file_if_none_provided()
        {
            var items = new[] { CreateGitItem("file1.foo", false, false, true), CreateGitItem("file2.txt", false, false, true) };

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].SelectedImageKey.Should().BeEmpty();
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
                _iconProvider.Received(1).Get(Arg.Any<string>(), items[i].Name);
            }
            _imageList.Images.Count.Should().Be(0);
        }

        [Test]
        public void LoadItemsInTreeView_should_add_icon_for_file_extension_only_once()
        {
            var items = new[] { CreateGitItem("file1.txt", false, false, true), CreateGitItem("file2.txt", false, false, true) };
            var image = Resources.cow_head;
            _iconProvider.Get(Arg.Any<string>(), Arg.Is<string>(x => x.EndsWith(".txt"))).Returns(image);

            _controller.LoadItemsInTreeView(items, _rootNode.Nodes, _imageList.Images);

            _rootNode.Nodes.Count.Should().Be(items.Length);
            for (int i = 0; i < items.Length - 1; i++)
            {
                _rootNode.Nodes[i].Text.Should().Be(items[i].Name);
                _rootNode.Nodes[i].ImageKey.Should().Be(".txt");
                _rootNode.Nodes[i].SelectedImageKey.Should().Be(".txt");
                _rootNode.Nodes[i].Nodes.Count.Should().Be(0);
                _iconProvider.Received(1).Get(Arg.Any<string>(), items[i].Name);
            }
            _imageList.Images.Count.Should().Be(1);
        }


        private IGitItem CreateGitItem(string name, bool isTree, bool isCommit, bool isBlol)
        {
            var item = new GitItem(new GitModule(""))
            {
                Name = name,
                FileName = isBlol ? name : "",
                ItemType = isTree ? "tree" : isBlol ? "blob" : isCommit ? "commit" : "",
            };
            return item;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        private class MockGitItem : IGitItem
        {
            public MockGitItem(string name)
            {
                Name = name;
            }

            public string Guid => System.Guid.NewGuid().ToString("N");
            public bool IsBlob { get; }
            public bool IsCommit { get; }
            public bool IsTree { get; }
            public string Name { get; }
            public IEnumerable<IGitItem> SubItems { get; }
        }
    }
}
